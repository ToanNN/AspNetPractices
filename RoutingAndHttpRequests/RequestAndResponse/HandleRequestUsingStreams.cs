using Microsoft.AspNetCore.SignalR.Protocol;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace RoutingAndHttpRequests.RequestAndResponse;

public class HandleRequestUsingStreams
{
    private async Task<List<string>> GetStringsFromStream(Stream requestBody)
    {
        var builder = new StringBuilder();

        //rent a shared buffer to write the request body to
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        while (true)
        {
            var byteRead = await requestBody.ReadAsync(buffer, 0, buffer.Length);
            if (byteRead == 0)
            {
                break;
            }

            var encodedValue = Encoding.UTF8.GetString(buffer, 0, byteRead);
            builder.Append(encodedValue);
        }

        ArrayPool<byte>.Shared.Return(buffer);
        var entireRequestBody = builder.ToString();

        // Split on \n in the string.
        return new List<string>(entireRequestBody.Split("\n"));
    }

    private async Task<List<string>> GetStringsFromPipe(PipeReader reader)
    {
        var result = new List<string>();

        while (true)
        {
            ReadResult readerResult = await reader.ReadAsync();
            var buffer = readerResult.Buffer;
            SequencePosition? position = null;

            do
            {
                // Look for a EOL in the buffer
                position = buffer.PositionOf((byte)'\n');
                if (position != null)
                {
                    var readOnlySequence = buffer.Slice(0, position.Value);
                    AddStringToList(readOnlySequence, result);

                    // Skip the line + the \n character (basically position)
                    buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                }
            } while (position != null);

            if (readerResult.IsCompleted && buffer.Length > 0)
            {
                AddStringToList( buffer, result );
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            // At this point, buffer will be updated to point one byte after the last
            // \n character.
            if (readerResult.IsCompleted)
            {
                break;
            }
        }
        return result;
    }

    private static void AddStringToList(ReadOnlySequence<byte> readOnlySequence, List<string> result)
    {
        ReadOnlySpan<byte> span = readOnlySequence.IsSingleSegment
            ? readOnlySequence.First.Span
            : readOnlySequence.ToArray().AsSpan();
        result.Add(Encoding.UTF8.GetString(span));
    }
}