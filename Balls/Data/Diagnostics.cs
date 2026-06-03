using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace ConcurrentProgramming.Data
{
    internal readonly record struct DiagnosticRecord(long TimestampTicks, int BallId, double X, double Y, double Vx, double Vy);

    internal interface IDiagnosticLogger : IDisposable
    {
        int DroppedRecords { get; }

        bool TryLog(DiagnosticRecord record);
    }

    internal static class DiagnosticLogSerializer
    {
        internal static string Serialize(DiagnosticRecord record)
        {
            return string.Create(
              CultureInfo.InvariantCulture,
              $"{record.TimestampTicks};{record.BallId};{record.X:F3};{record.Y:F3};{record.Vx:F3};{record.Vy:F3}");
        }
    }

    internal sealed class FileDiagnosticLogger : IDiagnosticLogger
    {
        internal const int DefaultCapacity = 1024;

        internal FileDiagnosticLogger(string filePath)
          : this(filePath, DefaultCapacity)
        { }

        internal FileDiagnosticLogger(string filePath, int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Buffer = new BlockingCollection<string>(capacity);
            WriterTask = Task.Run(() => WriteLoop(filePath));
        }

        public int DroppedRecords => DroppedRecordsBackingField;

        public bool TryLog(DiagnosticRecord record)
        {
            string serialized = DiagnosticLogSerializer.Serialize(record);
            bool accepted = Buffer.TryAdd(serialized);
            if (!accepted)
                Interlocked.Increment(ref DroppedRecordsBackingField);
            return accepted;
        }

        public void Dispose()
        {
            Buffer.CompleteAdding();
            try
            {
                WriterTask.Wait(TimeSpan.FromSeconds(2));
            }
            finally
            {
                Buffer.Dispose();
            }
        }

        private int DroppedRecordsBackingField;
        private readonly BlockingCollection<string> Buffer;
        private readonly Task WriterTask;

        private void WriteLoop(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? AppContext.BaseDirectory);
            using StreamWriter writer = new(filePath, append: true, Encoding.ASCII);
            foreach (string record in Buffer.GetConsumingEnumerable())
                writer.WriteLine(record);
        }
    }
}
