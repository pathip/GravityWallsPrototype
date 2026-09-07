using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// TurnManager — ระบบ Turn Player หลัก
// เชื่อมกับ DeckManager, DrawSystem, AddCardSystem, DiceRoller, PlayerData
//
// Flow ของแต่ละ Turn:
//   1. ตรวจว่าเกมจบหรือยัง
//   2. ดึง Current Player
//   3. เช็ค skipTurns — ถ้ามีให้ข้าม
//   4. Tick Status Effects (poison, regen ฯลฯ)
//   5. จั่วการ์ดให้ผู้เล่น (DrawOnTurnStart)
//   6. ยิง Event แจ้ง UI และ System อื่น ๆ
//   7. รอ EndTurn() — ผู้เล่นกดหรือ AI เรียก
//   8. Discard Hand แล้วเลื่อนไปคนถัดไป

public class TurnManager : MonoBehaviour
{
    // ==========================================
    // DEPENDENCIES — ลาก assign ใน Inspector
    // ==========================================
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private DrawSystem drawSystem;
    [SerializeField] private AddCardSystem addCardSystem;
    [SerializeField] private DiceRoller diceRoller;      // optional — ถ้าเกมใช้ลูกเต๋า

    // ==========================================
    // SETTINGS
    // ==========================================
    [Header("Turn Settings")]
    [SerializeField] private int drawPerTurn = 1;
    [SerializeField] private bool autoDiscardOnEnd = true;
    [SerializeField] private float turnDelay = 0.5f;

    [Header("Dice Settings")]
    [SerializeField] private bool rollDiceOnTurnStart = false; // ทอยต้น Turn อัตโนมัติ
    [SerializeField] private int diceCount = 1;     // จำนวนลูก
    [SerializeField] private int diceSides = 6;     // หน้าลูกเต๋า

    // ==========================================
    // STATE
    // ==========================================
    private List<PlayerData> players = new List<PlayerData>();
    private int currentIndex = 0;
    private int round = 1;
    private bool gameOver = false;
    private bool isTurnRunning = false;

    // ==========================================
    // EVENTS — UI หรือ System อื่น subscribe ได้
    // ==========================================
    public event System.Action<PlayerData, int> OnTurnStart;   // (player, round)
    public event System.Action<PlayerData> OnTurnEnd;     // (player)
    public event System.Action<PlayerData> OnPlayerEliminated;
    public event System.Action<PlayerData> OnGameOver;    // (winner)

    // ==========================================
    // SETUP
    // ==========================================
    void Start()
    {
        // ตัวอย่างสร้างผู้เล่น — ในเกมจริงรับจากภายนอก
        AddPlayer("Player 1", 20);
        AddPlayer("Player 2", 20);

        StartGame();
    }

    public void AddPlayer(string name, int hp)
    {
        players.Add(new PlayerData(name, hp));
    }

    public void StartGame()
    {
        currentIndex = 0;
        round = 1;
        gameOver = false;

        deckManager.BuildDeck();
        deckManager.Shuffle();

        Debug.Log($"[Game] เริ่มเกม — ผู้เล่น {players.Count} คน");
        StartCoroutine(RunTurn());
    }

    // ==========================================
    // TURN LOOP (Coroutine)
    // ==========================================
    private IEnumerator RunTurn()
    {
        while (!gameOver)
        {
            // รอ delay ก่อนเริ่ม Turn
            yield return new WaitForSeconds(turnDelay);

            // 1. เช็คว่าเกมจบหรือยัง
            if (CheckGameOver()) yield break;

            PlayerData current = players[currentIndex];

            // 2. เช็ค Skip Turn
            if (current.skipTurns > 0)
            {
                current.skipTurns--;
                Debug.Log($"[Skip] {current.playerName} ถูก Skip (เหลือ {current.skipTurns})");
                NextIndex();
                continue;
            }

            // 3. เช็คว่า Player ยัง Active
            if (!current.IsActive)
            {
                NextIndex();
                continue;
            }

            isTurnRunning = true;

            // 4. Tick Status Effects
            current.TickEffects();

            // 5. จั่วการ์ดต้น Turn
            if (drawPerTurn > 0)
                DrawForPlayer(current, drawPerTurn);

            // 5b. ทอยลูกเต๋าต้น Turn (ถ้าเปิดใช้)
            if (rollDiceOnTurnStart && diceRoller != null)
                RollDiceForPlayer(current, diceCount, diceSides);

            // 6. แจ้ง Event OnTurnStart
            Debug.Log($"[Turn] Round {round} — {current.playerName} เริ่ม Turn");
            OnTurnStart?.Invoke(current, round);

            // 7. รอจนกว่า EndTurn() จะถูกเรียก
            yield return new WaitUntil(() => !isTurnRunning);
        }
    }

    // ==========================================
    // END TURN — ผู้เล่นหรือ AI เรียกเมื่อจบ Turn
    // ==========================================
    public void EndTurn()
    {
        if (!isTurnRunning)
        {
            Debug.LogWarning("[TurnManager] ไม่มี Turn ที่กำลังรัน");
            return;
        }

        PlayerData current = players[currentIndex];

        // Discard Hand อัตโนมัติ
        if (autoDiscardOnEnd)
            DiscardPlayerHand(current);

        Debug.Log($"[Turn] {current.playerName} จบ Turn");
        OnTurnEnd?.Invoke(current);

        // เลื่อนไปผู้เล่นถัดไป
        NextIndex();
        isTurnRunning = false;
    }

    // ==========================================
    // NEXT PLAYER (Circular)
    // ==========================================
    private void NextIndex()
    {
        int prev = currentIndex;
        currentIndex = (currentIndex + 1) % players.Count;

        // ถ้าวนกลับมาที่ 0 = รอบใหม่
        if (currentIndex == 0 && prev != 0)
        {
            round++;
            Debug.Log($"[Round] เริ่ม Round {round}");

            // Reshuffle Discard ถ้า Deck เหลือน้อย
            if (deckManager.DeckCount < players.Count * drawPerTurn)
            {
                Debug.Log("[Deck] Deck เหลือน้อย — Reshuffle Discard");
                deckManager.ReshuffleDiscard();
            }
        }
    }

    // ==========================================
    // DRAW FOR PLAYER
    // เชื่อมกับ DeckManager และเพิ่มเข้า Hand ของผู้เล่น
    // ==========================================
    private void DrawForPlayer(PlayerData player, int count)
    {
        var drawn = deckManager.DrawCards(count);
        if (drawn == null)
        {
            Debug.LogWarning("[Draw] Deck ว่าง ไม่สามารถจั่วได้");
            return;
        }

        player.hand.AddRange(drawn);
        Debug.Log($"[Draw] {player.playerName} จั่ว {drawn.Count} ใบ " +
                  $"(Hand: {player.hand.Count} ใบ)");
    }

    // ==========================================
    // ROLL DICE FOR PLAYER
    // เรียกจาก TurnManager ต้น Turn หรือเรียกตรงจากภายนอก
    // ==========================================
    public DiceResult RollDiceForPlayer(PlayerData player,
                                        int nDice = 1,
                                        int sides = 6,
                                        int modifier = 0)
    {
        if (diceRoller == null)
        {
            Debug.LogWarning("[TurnManager] ไม่ได้ผูก DiceRoller ใน Inspector");
            return null;
        }
        return diceRoller.Roll(player, nDice, sides, modifier);
    }

    // ==========================================
    // DISCARD PLAYER HAND
    // เชื่อมกับ DeckManager.AddToDiscard()
    // ==========================================
    private void DiscardPlayerHand(PlayerData player)
    {
        if (player.hand.Count == 0) return;
        deckManager.AddToDiscard(player.hand);
        player.hand.Clear();
    }

    // ==========================================
    // GAME OVER CHECK
    // ==========================================
    private bool CheckGameOver()
    {
        // กรณีมีผู้เล่นหลายคน — จบเมื่อเหลือคนเดียว
        var alive = players.FindAll(p => p.IsActive);

        if (alive.Count <= 1)
        {
            gameOver = true;
            var winner = alive.Count == 1 ? alive[0] : null;
            string winnerName = winner != null ? winner.playerName : "ไม่มี";
            Debug.Log($"[GameOver] ผู้ชนะ: {winnerName}");
            OnGameOver?.Invoke(winner);
            return true;
        }
        return false;
    }

    // ==========================================
    // SKIP & ELIMINATE (เรียกจากภายนอก)
    // ==========================================

    // บังคับ Skip Turn ผู้เล่นคนใดก็ได้
    public void SkipPlayer(PlayerData player, int turns = 1)
    {
        player.skipTurns += turns;
    }

    // Eliminate ผู้เล่น
    public void EliminatePlayer(PlayerData player)
    {
        player.isEliminated = true;
        DiscardPlayerHand(player);
        Debug.Log($"[Eliminate] {player.playerName} ถูกตัดออกจากเกม");
        OnPlayerEliminated?.Invoke(player);
    }

    // ==========================================
    // PROPERTIES
    // ==========================================
    public PlayerData CurrentPlayer => players.Count > 0 ? players[currentIndex] : null;
    public int CurrentRound => round;
    public bool IsGameOver => gameOver;
    public bool IsTurnRunning => isTurnRunning;
    public List<PlayerData> Players => players;

    // คืน List ผู้เล่นที่ยังเล่นอยู่ — ใช้โดย CoinFlipUIHandler (Group Flip)
    public List<PlayerData> GetActivePlayers()
    {
        var active = new List<PlayerData>();
        foreach (var p in players)
            if (!p.isEliminated && p.hp > 0)
                active.Add(p);
        return active;
    }
}
