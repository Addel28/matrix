using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class Program
{
    static async Task<int[,]> MultiplyMatricesAsync(int[,] matrixA, int[,] matrixB)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int rowsB = matrixB.GetLength(0);
        int colsB = matrixB.GetLength(1);

        if (colsA != rowsB)
        {
            throw new InvalidOperationException("Количество столбцов в матрице A должно быть равно количеству строк в матрице B");
        }

        int[,] result = new int[rowsA, colsB];

        await Task.Run(() =>
        {
            for (int i = 0; i < rowsA; i++)
            {
                for (int j = 0; j < colsB; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < colsA; k++)
                    {
                        result[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
        });

        return result;
    }

    static async Task PipelineProcessing()
    {
        var bufferBlock = new BufferBlock<int>();
        var transformBlock = new TransformBlock<int, int>(async item =>
        {
            await Task.Delay(100);
            return item * item;
        });
        var actionBlock = new ActionBlock<int>(async item =>
        {
            await Task.Delay(50);
            Console.WriteLine($"Обработанный предмет: {item}");
        });

        bufferBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
        transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        for (int i = 1; i <= 10; i++)
        {
            await bufferBlock.SendAsync(i);
        }
        bufferBlock.Complete();

        await actionBlock.Completion;
    }

    static async void Matrix(int[,] matrix, int x, int y) 
    {
        Random r = new Random();
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                matrix[i, j] = r.Next(0, 10);
            }
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("Введите кол-во столбцов в матрице");
        int x = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Введите кол-во строк в матрице");
        int y = Convert.ToInt32(Console.ReadLine());
        int[,] matrixA = new int[x, y];
        int[,] matrixB = new int[y, x];
        Matrix(matrixA, x,y);
        Matrix(matrixB, y,x);
        //Random r = new Random();
        //for (int i = 0; i < x; i++)
        //{
        //    for (int j = 0; j < y; j++)
        //    {
        //        matrixA[i, j] = r.Next(0, 10);
        //    }
        //}
        //for (int i = 0; i < y; i++)
        //{
        //    for (int j = 0; j < x; j++)
        //    {
        //        matrixB[i, j] = r.Next(0, 10);
        //    }
        //}

        Console.WriteLine("Асинхронное умножение матриц...");
        int[,] result = await MultiplyMatricesAsync(matrixB, matrixA);

        Console.WriteLine("Матрица А:");
        for (int i = 0; i < matrixA.GetLength(0); i++)
        {
            for (int j = 0; j < matrixA.GetLength(1); j++)
            {
                Console.Write(matrixA[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Матрица В:");
        for (int i = 0; i < matrixB.GetLength(0); i++)
        {
            for (int j = 0; j < matrixB.GetLength(1); j++)
            {
                Console.Write(matrixB[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Результат умножения матрицы:");
        for (int i = 0; i < result.GetLength(0); i++)
        {
            for (int j = 0; j < result.GetLength(1); j++)
            {
                Console.Write(result[i, j] + " ");
            }
            Console.WriteLine();
        }

        Console.WriteLine("Обработка конвейера асинхронно...");
        await PipelineProcessing();
    }
}