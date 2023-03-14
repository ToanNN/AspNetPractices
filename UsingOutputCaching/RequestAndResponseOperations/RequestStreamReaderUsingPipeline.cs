using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace UsingOutputCaching.RequestAndResponseOperations;

public class RequestStreamReaderUsingPipeline
{
    public async Task<List<string>> GetStringsFromPipe(PipeReader reader)
    {
        var results = new List<string>();
        while (true)
        {
            ReadResult readResult = await reader.ReadAsync();
            var buffer = readResult.Buffer;
            SequencePosition? position = null;

            do
            {
                // Look for a EOL in the buffer
                position = buffer.GetPosition((byte)'\n');
                //Get the position of '\n' from the buffer
                if (position != null)
                {
                    // read the byte sequence to the position of \n
                    var readOnlySequence = buffer.Slice(0, position.Value);

                    //Convert byte sequence to string and add to the result
                    AddStringToList(results, in readOnlySequence);

                    // Skip the read string and move to the next position after \n position
                    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                }
            } while (position != null);

            // End of the pipe
            if (readResult.IsCompleted && buffer.Length > 0)
            {
                AddStringToList(results, in buffer);
            }

            // Move the read cursor to the end of the buffer
            reader.AdvanceTo(buffer.Start, buffer.End);

            // At this point, buffer will be updated to point one byte after the last
            // \n character.
            if (readResult.IsCompleted)
            {
                break;
            }
        }

        return results;
    }

    private void AddStringToList(List<string> results, in ReadOnlySequence<byte> readOnlySequence)
    {
        ReadOnlySpan<byte> span = readOnlySequence.IsSingleSegment
            ? readOnlySequence.First.Span
            : readOnlySequence.ToArray().AsSpan();
        results.Add(Encoding.UTF8.GetString(span));
    }

    //This example fixes many issues that the streams implementations had:

    //There's no need for a string buffer because the PipeReader handles bytes that haven't been used.
    //    Encoded strings are directly added to the list of returned strings.
    //    Other than the ToArray call, and the memory used by the string, string creation is allocation free.
}