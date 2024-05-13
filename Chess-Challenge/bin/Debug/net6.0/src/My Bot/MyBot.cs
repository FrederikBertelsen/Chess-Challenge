using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChessChallenge.Application.APIHelpers;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;

namespace ChessChallenge.Example;

public class MyBot : IChessBot {
    Move bestmoveRoot = Move.NullMove;

    // https://www.chessprogramming.org/PeSTO%27s_Evaluation_Function
    int[] piecePhase = { 0, 0, 155, 305, 405, 1050, 0 };
    int[] pieceVal = { 0, 100, 310, 330, 500, 1000, 10000 };


    int[] tables = new int[12 * 64] {
        //PAWN MG
        100, 100, 100, 100, 100, 100, 100, 100,
        176, 214, 147, 194, 189, 214, 132, 77,
        82, 88, 106, 113, 150, 146, 110, 73,
        67, 93, 83, 95, 97, 92, 99, 63,
        55, 74, 80, 89, 94, 86, 90, 55,
        55, 70, 68, 69, 76, 81, 101, 66,
        52, 84, 66, 60, 69, 99, 117, 60,
        100, 100, 100, 100, 100, 100, 100, 100,
        //PAWN EG
        100, 100, 100, 100, 100, 100, 100, 100,
        277, 270, 252, 229, 240, 233, 264, 285,
        190, 197, 182, 168, 155, 150, 180, 181,
        128, 117, 108, 102, 93, 100, 110, 110,
        107, 101, 89, 85, 86, 83, 92, 91,
        96, 96, 85, 92, 88, 83, 85, 82,
        107, 99, 97, 97, 100, 89, 89, 84,
        100, 100, 100, 100, 100, 100, 100, 100,
        //KNIGHT MG
        116, 228, 271, 270, 338, 213, 278, 191,
        225, 247, 353, 331, 321, 360, 300, 281,
        258, 354, 343, 362, 389, 428, 375, 347,
        300, 332, 325, 360, 349, 379, 339, 333,
        298, 322, 325, 321, 337, 332, 332, 303,
        287, 297, 316, 319, 327, 320, 327, 294,
        276, 259, 300, 304, 308, 322, 296, 292,
        208, 290, 257, 274, 296, 284, 293, 284,
        //KNIGHT EG
        229, 236, 269, 250, 257, 249, 219, 188,
        252, 274, 263, 281, 273, 258, 260, 229,
        253, 264, 290, 289, 278, 275, 263, 243,
        267, 280, 299, 301, 299, 293, 285, 264,
        263, 273, 293, 301, 296, 293, 284, 261,
        258, 276, 278, 290, 287, 274, 260, 255,
        241, 259, 270, 277, 276, 262, 260, 237,
        253, 233, 258, 264, 261, 260, 234, 215,
        //BISHOP MG
        292, 338, 254, 283, 299, 294, 337, 323,
        316, 342, 319, 319, 360, 385, 343, 295,
        342, 377, 373, 374, 368, 392, 385, 363,
        332, 338, 356, 384, 370, 380, 337, 341,
        327, 354, 353, 366, 373, 346, 345, 341,
        335, 350, 351, 347, 352, 361, 350, 344,
        333, 354, 354, 339, 344, 353, 367, 333,
        309, 341, 342, 325, 334, 332, 302, 313,
        //BISHOP EG
        288, 278, 287, 292, 293, 290, 287, 277,
        289, 294, 301, 288, 296, 289, 294, 281,
        292, 289, 296, 292, 296, 300, 296, 293,
        293, 302, 305, 305, 306, 302, 296, 297,
        289, 293, 304, 308, 298, 301, 291, 288,
        285, 294, 304, 303, 306, 294, 290, 280,
        285, 284, 291, 299, 300, 290, 284, 271,
        277, 292, 286, 295, 294, 288, 290, 285,
        //ROOK MG
        493, 511, 487, 515, 514, 483, 485, 495,
        493, 498, 529, 534, 546, 544, 483, 508,
        465, 490, 499, 497, 483, 519, 531, 480,
        448, 464, 476, 495, 484, 506, 467, 455,
        442, 451, 468, 470, 476, 472, 498, 454,
        441, 461, 468, 465, 478, 481, 478, 452,
        443, 472, 467, 476, 483, 500, 487, 423,
        459, 463, 470, 479, 480, 480, 446, 458,
        //ROOK EG
        506, 500, 508, 502, 504, 507, 505, 503,
        505, 506, 502, 502, 491, 497, 506, 501,
        504, 503, 499, 500, 500, 495, 496, 496,
        503, 502, 510, 500, 502, 504, 500, 505,
        505, 509, 509, 506, 504, 503, 496, 495,
        500, 503, 500, 505, 498, 498, 499, 489,
        496, 495, 502, 505, 498, 498, 491, 499,
        492, 497, 498, 496, 493, 493, 497, 480,
        //QUEEN MG
        865, 902, 922, 911, 964, 948, 933, 928,
        886, 865, 903, 921, 888, 951, 923, 940,
        902, 901, 907, 919, 936, 978, 965, 966,
        881, 885, 897, 894, 898, 929, 906, 915,
        907, 884, 899, 896, 904, 906, 912, 911,
        895, 916, 900, 902, 904, 912, 924, 917,
        874, 899, 918, 908, 915, 924, 911, 906,
        906, 899, 906, 918, 898, 890, 878, 858,
        //QUEEN EG
        918, 937, 943, 945, 934, 926, 924, 942,
        907, 945, 946, 951, 982, 933, 928, 912,
        896, 921, 926, 967, 963, 937, 924, 915,
        926, 944, 939, 962, 983, 957, 981, 950,
        893, 949, 942, 970, 952, 956, 953, 936,
        911, 892, 933, 928, 934, 942, 934, 924,
        907, 898, 883, 903, 903, 893, 886, 888,
        886, 887, 890, 872, 916, 890, 906, 879,
        //KING MG
        -11, 70, 55, 31, -37, -16, 22, 22,
        37, 24, 25, 36, 16, 8, -12, -31,
        33, 26, 42, 11, 11, 40, 35, -2,
        0, -9, 1, -21, -20, -22, -15, -60,
        -25, 16, -27, -67, -81, -58, -40, -62,
        7, -2, -37, -77, -79, -60, -23, -26,
        12, 15, -13, -72, -56, -28, 15, 17,
        -6, 44, 29, -58, 8, -25, 34, 28,
        //KING EG
        -74, -43, -23, -25, -11, 10, 1, -12,
        -18, 6, 4, 9, 7, 26, 14, 8,
        -3, 6, 10, 6, 8, 24, 27, 3,
        -16, 8, 13, 20, 14, 19, 10, -3,
        -25, -14, 13, 20, 24, 15, 1, -15,
        -27, -10, 9, 20, 23, 14, 2, -12,
        -32, -17, 4, 14, 15, 5, -10, -22,
        -55, -40, -23, -6, -20, -8, -28, -47,
    };


    // https://www.chessprogramming.org/Transposition_Table
    struct TTEntry {
        public ulong key;
        public Move move;
        public int depth, score, bound;

        public TTEntry(ulong _key, Move _move, int _depth, int _score, int _bound) {
            key = _key;
            move = _move;
            depth = _depth;
            score = _score;
            bound = _bound;
        }
    }

    const int entries = (1 << 20);
    TTEntry[] tt = new TTEntry[entries];

    public int Evaluate(Board board) {
        int mg = 0, eg = 0, phase = 0;

        foreach (bool stm in new[] { true, false }) {
            for (var p = PieceType.Pawn; p <= PieceType.King; p++) {
                int piece = (int)p, ind;
                ulong mask = board.GetPieceBitboard(p, stm);
                while (mask != 0) {
                    ind = 128 * (piece - 1) + BitboardHelper.ClearAndGetIndexOfLSB(ref mask) ^ (stm ? 56 : 0);

                    mg += tables[ind] + pieceVal[piece] * 5;
                    eg += tables[ind + 64] + pieceVal[piece] * 5;

                    phase += piecePhase[piece];
                }
            }

            mg = -mg;
            eg = -eg;
        }

        int midgame = 5255;
        int endgame = 435;
        phase = Math.Min(1, Math.Max(0, (phase - midgame) / (endgame - midgame)));

        return mg + phase * (eg - mg);
    }

    // https://www.chessprogramming.org/Negamax
    // https://www.chessprogramming.org/Quiescence_Search
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Search(Board board, Timer timer, int alpha, int beta, int depth, int ply) {
        ulong key = board.ZobristKey;
        bool qsearch = depth <= 0;
        bool notRoot = ply > 0;
        int best = -30000;

        // Check for repetition (this is much more important than material and 50 move rule draws)
        if (notRoot && board.IsRepeatedPosition())
            return 0;

        TTEntry entry = tt[key % entries];

        // TT cutoffs
        if (notRoot && entry.key == key && entry.depth >= depth && (
                entry.bound == 3 // exact score
                || entry.bound == 2 && entry.score >= beta // lower bound, fail high
                || entry.bound == 1 && entry.score <= alpha // upper bound, fail low
            )) return entry.score;

        // Quiescence search is in the same function as negamax to save tokens
        if (qsearch) {
            best = Evaluate(board);
            if (best >= beta) return best;
            alpha = Math.Max(alpha, best);
        }

        // Generate moves, only captures in qsearch
        // Move[] moves = board.GetLegalMoves(qsearch);
        // int[] scores = new int[moves.Length];
        Span<Move> moves = stackalloc Move[128];
        board.GetLegalMovesNonAlloc(ref moves, qsearch);
        Span<int> scores = stackalloc int[moves.Length];

        // Score moves
        for (int i = 0; i < moves.Length; i++) {
            Move move = moves[i];
            // TT move
            if (move == entry.move) scores[i] = 1000000;
            // https://www.chessprogramming.org/MVV-LVA
            else if (move.IsCapture) scores[i] = 100 * (int)move.CapturePieceType - (int)move.MovePieceType;
        }

        Move bestMove = Move.NullMove;
        int origAlpha = alpha;

        // Search moves
        for (int i = 0; i < moves.Length; i++) {
            if (outOfTime) return 30000;

            // Incrementally sort moves
            for (int j = i + 1; j < moves.Length; j++) {
                if (scores[j] > scores[i])
                    (scores[i], scores[j], moves[i], moves[j]) = (scores[j], scores[i], moves[j], moves[i]);
            }

            Move move = moves[i];
            board.MakeMove(move);
            int score = -Search(board, timer, -beta, -alpha, depth - 1, ply + 1);
            board.UndoMove(move);

            // New best move
            if (score > best) {
                best = score;
                bestMove = move;
                if (ply == 0) bestmoveRoot = move;

                // Improve alpha
                alpha = Math.Max(alpha, score);

                // Fail-high
                if (alpha >= beta) break;
            }
        }

        // (Check/Stale)mate
        if (!qsearch && moves.Length == 0) return board.IsInCheck() ? -30000 + ply : 0;

        // Did we fail high/low or get an exact score?
        // Push to TT
        tt[key % entries] = new TTEntry(key, bestMove, depth, best, best >= beta ? 2 : best > origAlpha ? 3 : 1);

        return best;
    }

    Timer timer;
    bool outOfTime => timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / 30;

    public Move Think(Board board, Timer t) {
        timer = t;

        int depth = 1;
        // https://www.chessprogramming.org/Iterative_Deepening
        for (depth = 1; depth <= 50; depth++) {
            int score = Search(board, timer, -30000, 30000, depth, 0);

            // Out of time
            if (outOfTime)
                break;
        }

        Console.WriteLine(depth + " " + timer.MillisecondsElapsedThisTurn);


        return bestmoveRoot.IsNull ? board.GetLegalMoves()[0] : bestmoveRoot;
    }
}