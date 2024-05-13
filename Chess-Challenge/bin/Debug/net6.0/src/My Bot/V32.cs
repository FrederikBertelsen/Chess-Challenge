using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;
using System.Runtime.CompilerServices;

public class V32 : IChessBot {
    // V2  

    int startDepth = 3;

    float NegativeInfinity = -99999;

// Piece values: null, pawn, knight, bishop, rook, queen, king  
    int[] pieceValues = { 0, 100, 300, 320, 500, 900, 1000 };

    int[] mg_value = { 0, 82, 337, 365, 477, 1025, 0 };
    int[] eg_value = { 0, 94, 281, 297, 512, 936, 0 };

    List<int> SquareMap;

    void CreateMap(long[] array) {
        SquareMap = new List<int>();

        string strNumber = String.Join("", array);

        foreach (var str in Enumerable.Range(0, strNumber.Length / 3).Select(i => strNumber.Substring(i * 3, 3))
                     .ToList()) {
            int count = Int32.Parse(str.Substring(0, 1)), num = Int32.Parse(str.Substring(1, 2));

// if (num > 200) num = -1 * (num - 200);  
            if (num > 50) num = -1 * (num - 50);

            for (int j = 0; j < count; j++) {
                SquareMap.Add(num * 4);
// SquareMap.Add(num);  
            }
        }
    }

    int[] tables = {
// manhatten distance to center  
        6 * 4, 5 * 4, 4 * 4, 3 * 4, 3 * 4, 4 * 4, 5 * 4, 6 * 4,
        5 * 4, 4 * 4, 3 * 4, 2 * 4, 2 * 4, 3 * 4, 4 * 4, 5 * 4,
        4 * 4, 3 * 4, 2 * 4, 1 * 4, 1 * 4, 2 * 4, 3 * 4, 4 * 4,
        3 * 4, 2 * 4, 1 * 4, 0 * 4, 0 * 4, 1 * 4, 2 * 4, 3 * 4,
        3 * 4, 2 * 4, 1 * 4, 0 * 4, 0 * 4, 1 * 4, 2 * 4, 3 * 4,
        4 * 4, 3 * 4, 2 * 4, 1 * 4, 1 * 4, 2 * 4, 3 * 4, 4 * 4,
        5 * 4, 4 * 4, 3 * 4, 2 * 4, 2 * 4, 3 * 4, 4 * 4, 5 * 4,
        6 * 4, 5 * 4, 4 * 4, 3 * 4, 3 * 4, 4 * 4, 5 * 4, 6 * 4,

        0, 0, 0, 0, 0, 0, 0, 0,
        98, 134, 61, 95, 68, 126, 34, -11,
        -6, 7, 26, 31, 65, 56, 25, -20,
        -14, 13, 6, 21, 23, 12, 17, -23,
        -27, -2, -5, 12, 17, 6, 10, -25,
        -26, -4, -4, -10, 3, 3, 33, -12,
        -35, -1, -20, -23, -15, 24, 38, -22,
        0, 0, 0, 0, 0, 0, 0, 0,

        0, 0, 0, 0, 0, 0, 0, 0,
        178, 173, 158, 134, 147, 132, 165, 187,
        94, 100, 85, 67, 56, 53, 82, 84,
        32, 24, 13, 5, -2, 4, 17, 17,
        13, 9, -3, -7, -7, -8, 3, -1,
        4, 7, -6, 1, 0, -5, -1, -8,
        13, 8, 8, 10, 13, 0, 2, -7,
        0, 0, 0, 0, 0, 0, 0, 0,

        -167, -89, -34, -49, 61, -97, -15, -107,
        -73, -41, 72, 36, 23, 62, 7, -17,
        -47, 60, 37, 65, 84, 129, 73, 44,
        -9, 17, 19, 53, 37, 69, 18, 22,
        -13, 4, 16, 13, 28, 19, 21, -8,
        -23, -9, 12, 10, 19, 17, 25, -16,
        -29, -53, -12, -3, -1, 18, -14, -19,
        -105, -21, -58, -33, -17, -28, -19, -23,

        -58, -38, -13, -28, -31, -27, -63, -99,
        -25, -8, -25, -2, -9, -25, -24, -52,
        -24, -20, 10, 9, -1, -9, -19, -41,
        -17, 3, 22, 22, 22, 11, 8, -18,
        -18, -6, 16, 25, 16, 17, 4, -18,
        -23, -3, -1, 15, 10, -3, -20, -22,
        -42, -20, -10, -5, -2, -20, -23, -44,
        -29, -51, -23, -15, -22, -18, -50, -64,

        -29, 4, -82, -37, -25, -42, 7, -8,
        -26, 16, -18, -13, 30, 59, 18, -47,
        -16, 37, 43, 40, 35, 50, 37, -2,
        -4, 5, 19, 50, 37, 37, 7, -2,
        -6, 13, 13, 26, 34, 12, 10, 4,
        0, 15, 15, 15, 14, 27, 18, 10,
        4, 15, 16, 0, 7, 21, 33, 1,
        -33, -3, -14, -21, -13, -12, -39, -21,

        -14, -21, -11, -8, -7, -9, -17, -24,
        -8, -4, 7, -12, -3, -13, -4, -14,
        2, -8, 0, -1, -2, 6, 0, 4,
        -3, 9, 12, 9, 14, 10, 3, 2,
        -6, 3, 13, 19, 7, 10, -3, -9,
        -12, -3, 8, 10, 13, 3, -7, -15,
        -14, -18, -7, -1, 4, -9, -15, -27,
        -23, -9, -23, -5, -9, -16, -5, -17,

        32, 42, 32, 51, 63, 9, 31, 43,
        27, 32, 58, 62, 80, 67, 26, 44,
        -5, 19, 26, 36, 17, 45, 61, 16,
        -24, -11, 7, 26, 24, 35, -8, -20,
        -36, -26, -12, -1, 9, -7, 6, -23,
        -45, -25, -16, -17, 3, 0, -5, -33,
        -44, -16, -20, -9, -1, 11, -6, -71,
        -19, -13, 1, 17, 16, 7, -37, -26,

        13, 10, 18, 15, 12, 12, 8, 5,
        11, 13, 13, 11, -3, 3, 8, 3,
        7, 7, 7, 5, 4, -3, -5, -3,
        4, 3, 13, 1, 2, 1, -1, 2,
        3, 5, 8, 4, -5, -6, -8, -11,
        -4, 0, -5, -1, -7, -12, -8, -16,
        -6, -6, 0, 2, -9, -9, -11, -3,
        -9, 2, 3, -1, -5, -13, 4, -20,

        -28, 0, 29, 12, 59, 44, 43, 45,
        -24, -39, -5, 1, -16, 57, 28, 54,
        -13, -17, 7, 8, 29, 56, 47, 57,
        -27, -27, -16, -16, -1, 17, -2, 1,
        -9, -26, -9, -10, -2, -4, 3, -3,
        -14, 2, -11, -2, -5, 2, 14, 5,
        -35, -8, 11, 2, 8, 15, -3, 1,
        -1, -18, -9, 10, -15, -25, -31, -50,

        -9, 22, 22, 27, 27, 19, 10, 20,
        -17, 20, 32, 41, 58, 25, 30, 0,
        -20, 6, 9, 49, 47, 35, 19, 9,
        3, 22, 24, 45, 57, 40, 57, 36,
        -18, 28, 19, 47, 31, 34, 39, 23,
        -16, -27, 15, 6, 9, 17, 10, 5,
        -22, -23, -30, -16, -16, -23, -36, -32,
        -33, -28, -22, -43, -5, -32, -20, -41,

        -65, 23, 16, -15, -56, -34, 2, 13,
        29, -1, -20, -7, -8, -4, -38, -29,
        -9, 24, 2, -16, -20, 6, 22, -22,
        -17, -20, -12, -27, -30, -25, -14, -36,
        -49, -1, -27, -39, -46, -44, -33, -51,
        -14, -14, -22, -46, -44, -30, -15, -27,
        1, 7, -8, -64, -43, -16, 9, 8,
        -15, 36, 12, -54, 8, -28, 24, 14,

        -74, -35, -18, -18, -11, 15, 4, -17,
        -12, 17, 14, 17, 17, 38, 23, 11,
        10, 17, 23, 15, 20, 45, 44, 13,
        -8, 22, 24, 27, 26, 33, 26, 3,
        -18, -4, 21, 24, 27, 23, 9, -11,
        -19, -3, 11, 21, 23, 16, 7, -9,
        -27, -11, 4, 13, 14, 4, -5, -17,
        -53, -34, -21, -11, -28, -14, -24, -43,
    };

    public void convertTable(int[] intArray) {
        Console.WriteLine("Array Length: " + intArray.Length);

        string output = "";
        int concurrent = 1;
        int lastValue = intArray[0];

        for (int i = 1; i < intArray.Length; i++) {
// concurrent cant be over 10, as then it would fill more than 1 decimal  
            if (intArray[i] == lastValue && concurrent < 8) {
                concurrent++;
            }
            else {
                output += concurrent + convertValue(lastValue);
// Console.WriteLine(concurrent + " & " + lastValue + " -> " + concurrent + convertValue(lastValue) + " ");  

                concurrent = 1;
                lastValue = intArray[i];
            }
        }

        output += concurrent + convertValue(lastValue);
// Console.WriteLine(concurrent + " & " + lastValue + " -> " + concurrent + convertValue(lastValue) + " ");  
// Console.WriteLine("Number Count: " + output.Length/4);  


        int length;
        for (int i = 0; i < output.Length; i += length) {
            length = 19;

            if (i + length >= output.Length) {
                Console.WriteLine(output.Substring(i, output.Length - i) + ", ");
                return;
            }

            while (output.ToCharArray()[i + length] == '0') {
                length--;
            }

// slice = output.Substring(i, length);  
            Console.Write(output.Substring(i, length) + ", ");
        }
    }

    public string convertValue(int i) {
        i = (int)Math.Round((double)i / 4);

        if (i < 0) i = 50 + -1 * i;
        return i.ToString().PadLeft(2, '0');


// if (i < 0) i = 200 + -1 * i;  
// return i.ToString().PadLeft(3, '0');  
    }

    public Move Think(Board board, Timer timer) {
        var moves = board.GetLegalMoves();
        int depth = startDepth;
        
// if (moves.Length < 20) depth++;  
        // if (moves.Length < 10) depth++;
// if (moves.Length < 5) depth++;  

// Console.WriteLine(moves.Length +" -> " + depth);  


        convertTable(tables);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        if (SquareMap == null)
            CreateMap(new[] {
                106105104203104105, 106105104103202103, 104105104103102201, 102103104103102101, 200101102203102101,
                200101102103104103, 102201102103104105, 104103202103104105, 106105104203104105, 1068001241341151241,
                1713210815315210210, 6108116114106155154, 1031021051061031041, 5615710015110310410, 2102156156251152201,
                1081531591001551561, 5410611015680080014, 4143140134137133141, 1471241251211171141, 1312012110810610310,
                1100101204103102151, 2521521011001011021, 5210010015110015210, 3202102103100100152, 8001921721581621151,
                7415417716816011810, 9106116102154162115, 1091161211321181111, 5210410511310911710, 4106153101104103107,
                105105152156152103, 1021051041061541571, 6315315110010415415, 5176155164158154157, 1551561641601531571,
                5815716617515615215, 6100152156156163156, 1551021021001521551, 6015410130610310225, 4152104106104104101,
                1541561511001041021, 5115515616015515215, 1100155156161157163, 1561541561541621661, 5710117015915616010,
                2152156104154153108, 1151041621541091111, 1010911210910015110, 1105112209102100152, 203106108103102101,
                100304104107104102, 101104104100102105, 1081001581511541551, 5315316015515415515, 3152152152154156152,
                1511021531511531511, 5410015210010010010, 2100101151102103102, 104102101100152101, 1031051021021511521,
                5315110210210310115, 2154154154152100101, 1521541571561521561, 511521541511541081, 1010811311610210811,
                1107108114116120117, 106111151105106109, 104111115104156153, 1021061061091521551, 5915615310010215210,
                2156161156154154101, 1001511581611541551, 5210010315216815515, 3100104104102159156, 103102104104203102,
                101103203103151101, 1021013021011011511, 51151101101103100, 100100100100101101, 1021011511521521531,
                5110015110015215315, 2154252100100252153, 1511521001011001511, 5310115515710010710, 3115111111111156160,
                1511001541141071141, 5315410210210711411, 2114257254100104100, 1001521561521521001, 51101151154100153,
                1001511001041011591, 52103100102104151, 1001001541521021541, 5615816215220620710, 5102105154105108110,
                114106108100155102, 102112112109105102, 1011061061111141101, 1410915410710511210, 8108110106154157104,
                1021021041021011561, 5615825415615915815, 8157156161151158155, 1601661061041541641, 5810010310710015515,
                2152151160157152106, 1001541551021061561, 5415515315715815615, 4159162100157160162, 1611581632541561621,
                6115815415710010215, 2166161154102102154, 109103164102157106, 104168159254153104, 1011541531041042041,
                1010610310210410610, 4105111111103152106, 1061071061081061011, 5415110510610710610, 2153155151103105106,
                104102152157153101, 1031041011511541631, 5815515315715415616, 1,
            });

        int StartColor = board.IsWhiteToMove ? -1 : 1;

        float bestMoveValue = -9999999;
        Move bestMove = new Move();
        foreach (var move in moves) {
            board.MakeMove(move);

            float newMoveValue = -Search(board, depth, NegativeInfinity, 99999, StartColor);

            if (newMoveValue > bestMoveValue) {
                bestMoveValue = newMoveValue;
                bestMove = move;
            }

            board.UndoMove(move);
        }

// if (turnCount == 0) Console.WriteLine("RESET");  

// turnCount++;  
        Console.WriteLine("EVAL: " + Eval(board, -StartColor));
// Console.WriteLine("Searched Positions: " + evalCount);  

// var before = Eval(board, StartColor);  
// board.MakeMove(bestMove);  
// var after = Eval(board, StartColor);  
// Console.WriteLine(before + " -> " + after);  

// Console.WriteLine();  
// Console.WriteLine(bestMove.ToString());  
        return bestMove;
    }

    public float Search(Board board, int depth, float alpha, float beta, int color) {
        float bestEval = NegativeInfinity;

        bool captureOnly = depth <= 0;
        if (captureOnly) {
            bestEval = Eval(board, color);
// if (depth == 0) return bestEval;  
            if (bestEval >= beta)
                return beta;
            if (bestEval > alpha)
                alpha = bestEval;
        }

// sort moves eval-guess  
        foreach (var move in board.GetLegalMoves(captureOnly).OrderBy(move => {
                     float moveScoreGuess = 0;
                     if (move.CapturePieceType != PieceType.None)
                         moveScoreGuess = 10 * pieceValues[(int)move.CapturePieceType] -
                                          pieceValues[(int)move.MovePieceType];
                     if (move.IsPromotion) moveScoreGuess += 900;
                     return -moveScoreGuess;
                 }).ToList()) {
            board.MakeMove(move);

            float newEval = -Search(board, depth - 1, -beta, -alpha, -color);
            bestEval = Math.Max(bestEval, newEval);

            board.UndoMove(move);

            if (newEval >= beta)
                return beta;
            alpha = Math.Max(alpha, bestEval);

            if (alpha >= beta && depth > 0) break;
        }

        return bestEval;
    }

    int[] gamephaseInc = { 0, 0, 1, 1, 2, 4, 0 };

    float Eval(Board board, int color) {
// int whiteSum = 0, blackSum = 0;  
        int mgWhiteSum = 0, egWhiteSum = 0, mgBlackSum = 0, egBlackSum = 0, gamePhase = 0;

        var allPieceLists = board.GetAllPieceLists();
        foreach (var pieceList in allPieceLists) {
            foreach (var piece in pieceList) {
                int pieceTypeInt = (int)piece.PieceType;
                var square = piece.Square;
                int index = square.Index;

// invert index if we are white  
                if (piece.IsWhite) index = (7 - square.Rank) * 8 + square.File;

// calculate the index for midgame and endgame tables indices  
                var mgIndex = (pieceTypeInt * 2 - 1) * 64 + index;
                var egIndex = (pieceTypeInt * 2) * 64 + index;

// piece value plus square value  
                int mgPieceValue = mg_value[pieceTypeInt] + SquareMap[mgIndex];
                int egPieceValue = eg_value[pieceTypeInt] + SquareMap[egIndex];

                if (piece.IsWhite) {
                    mgWhiteSum += mgPieceValue;
                    egWhiteSum += egPieceValue;
                }
                else {
                    mgBlackSum += mgPieceValue;
                    egBlackSum += egPieceValue;
                }

                gamePhase += gamephaseInc[pieceTypeInt];
            }
        }

        int mgScore = color * (mgWhiteSum - mgBlackSum);
        int egScore = color * (egWhiteSum - egBlackSum);

        int mgPhase = gamePhase;
        if (mgPhase > 24) mgPhase = 24; /* in case of early promotion */
        int egPhase = 24 - mgPhase;

        return (mgScore * mgPhase + egScore * egPhase) / 24
               + (board.IsInCheckmate() ? color * 10000000 : 0)
               + (board.IsInCheck() ? color * 300 : 0)
               + (board.IsDraw() ? color * NegativeInfinity : 0);
    }
}