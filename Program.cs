using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Balls
{
    internal static class Program
    {
        private static readonly Predicate<String> isDigitFrom0To4 = value => value.Length == 1 && Char.IsDigit(value[0]) && value.CompareTo("5") == -1;

        private static readonly Predicate<String> isDigitFrom1To9 = value => value.Length == 1 && Char.IsDigit(value[0]) && value != "0";

        private static readonly String enterBallsNumberMessage = "Введите количество шариков (от 1 до 9)";

        private static readonly String defaultIncorrectMessage = "Введено неверное значение!";

        private static readonly Int32 defaultTimeout = 100;

        private static readonly Int32 defaultFieldWidth = 75;

        private static readonly Int32 defaultFieldHeight = 25;

        private static readonly Char defaultFiller = '.';

        private static readonly Int32 defaultStep = 3;

        private static Object locker;

        private static Random random;

        private static Int32 ballsNumber;

        private static Int32[] threadPriorities;

        private static Char[,] field;

        private static Coordinate[] ballsCoordinates;

        private static void Main(String[] args)
        {
            Initialize();
            for (Int32 i = 0; i < ballsNumber; ++i)
            {
                Int32 j = i;
                Thread thread = new Thread(() =>
                {
                    while(true)
                    {
                        lock (locker)
                        {
                            RedrawField();
                            if (!MoveBall(j))
                                break;
                            Thread.Sleep(defaultTimeout);
                        }
                    }
                });
                thread.Priority = (ThreadPriority)threadPriorities[j];
                thread.Start();
            }
        }

        private static void Initialize()
        {
            ballsNumber = ReadBallsNumber();
            threadPriorities = new Int32[ballsNumber];
            for(Int32 i = 0; i < ballsNumber; ++i)
                threadPriorities[i] = ReadThreadPriority($"Введите приоритет потока {i} (от 0 до 4)");
            field = new Char[defaultFieldHeight, defaultFieldWidth];
            for(Int32 i = 0; i < defaultFieldHeight; ++i)
            for(Int32 j = 0; j < defaultFieldWidth; ++j)
                field[i, j] = defaultFiller;
            random = new Random();
            ballsCoordinates = new Coordinate[ballsNumber];
            for (Int32 i = 0; i < ballsNumber; ++i)
            {
                ballsCoordinates[i] = GetStartingCoordinate();
                field[ballsCoordinates[i].Y, ballsCoordinates[i].X] = i.ToString().Single();
            }
            locker = new Object();
        }

        private static void RedrawField()
        {
            StringBuilder builder = new StringBuilder((defaultFieldWidth + 1) * defaultFieldHeight);
            for(Int32 i = 0; i < defaultFieldHeight; ++i)
            {
                for(Int32 j = 0; j < defaultFieldWidth; ++j)
                    builder.Append(field[i, j]);
                builder.AppendLine();
            }
            Console.Clear();
            Console.WriteLine(builder.ToString());
        }

        private static Coordinate GetStartingCoordinate()
        {
            while (true)
            {
                Coordinate coordinate = new Coordinate(random.Next(defaultFieldWidth), random.Next(defaultFieldHeight));
                if (!ballsCoordinates.Contains(coordinate))
                    return coordinate;
            }
        }

        private static Coordinate ShiftCoordinate(Coordinate coordinate)
        {
            return new Coordinate(coordinate.X + random.Next(-defaultStep, defaultStep), coordinate.Y + random.Next(-defaultStep, defaultStep));
        }

        private static Coordinate GetFreeShiftedCoordinate(Coordinate coordinate)
        {
            while (true)
            {
                Coordinate shifted = ShiftCoordinate(coordinate);
                if (!ballsCoordinates.Contains(shifted))
                    return shifted;
            }
        }

        private static Boolean MoveBall(Int32 ballNumber)
        {
            field[ballsCoordinates[ballNumber].Y, ballsCoordinates[ballNumber].X] = defaultFiller;
            ballsCoordinates[ballNumber] = GetFreeShiftedCoordinate(ballsCoordinates[ballNumber]);
            if(ballsCoordinates[ballNumber].X >= 0 && ballsCoordinates[ballNumber].X < defaultFieldWidth &&
               ballsCoordinates[ballNumber].Y >= 0 && ballsCoordinates[ballNumber].Y < defaultFieldHeight)
            {
                field[ballsCoordinates[ballNumber].Y, ballsCoordinates[ballNumber].X] = ballNumber.ToString().Single();
                return true;
            }
            return false;
        }

        private static Int32 ReadBallsNumber()
        {
            return int.Parse(ReadValidString(isDigitFrom1To9, enterBallsNumberMessage, defaultIncorrectMessage));
        }

        private static Int32 ReadThreadPriority(String initialMessage)
        {
            return int.Parse(ReadValidString(isDigitFrom0To4, initialMessage, defaultIncorrectMessage));
        }

        private static String ReadValidString(Predicate<String> predicate, String initialMessage, String incorrectMessage)
        {
            Console.WriteLine(initialMessage);
            do
            {
                String input = Console.ReadLine();
                Console.WriteLine();
                if(predicate.Invoke(input))
                    return input;
                Console.WriteLine(incorrectMessage);
            } while(true);
        }
    }

    internal struct Coordinate : IEquatable<Coordinate>
    {
        public readonly Int32 X;

        public readonly Int32 Y;

        public Coordinate(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }

        public override Boolean Equals(Object obj)
        {
            return obj is Coordinate other && Equals(other);
        }

        public Boolean Equals(Coordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }
}
