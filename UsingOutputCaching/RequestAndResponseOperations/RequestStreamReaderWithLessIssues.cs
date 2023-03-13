using System.Buffers;
using System.Text;

namespace UsingOutputCaching.RequestAndResponseOperations;

//Suppose the goal is to create a middleware that reads the entire request body as a list of strings, splitting on new lines.
public class RequestStreamReaderWithLessIssues
{
    public async Task<List<string>> GetStringsFromStream(Stream stream)
    {
        StringBuilder builder = new StringBuilder();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        List<string> results = new List<string>();

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                results.Add(builder.ToString());
                break;
            }

            var prevIndex = 0;
            int index;
            while (true)
            {
                index = Array.IndexOf(buffer, (byte)'\n', prevIndex);
                // we have not found the newline character, add all of the bytes read to the buffer
                if (index == -1)
                {
                    break;
                }

                // We have found new line delimiter
                var encodedString = Encoding.UTF8.GetString(buffer, prevIndex, index - prevIndex);

                // if the buffer contains previously read data, return all data
                if (builder.Length > 0)
                {
                    results.Add(builder.Append(encodedString).ToString());
                    builder.Clear();
                }
                // else just add the encoded string directly to the results
                else
                {
                    results.Add(encodedString);
                }

                //Skip past the last \n
                prevIndex = index + 1;
            }

            var remainingString = Encoding.UTF8.GetString(buffer, prevIndex, bytesRead - prevIndex);
            builder.Append(remainingString);
        }

        ArrayPool<byte>.Shared.Return(buffer);

        return results;
    }

    //This preceding example:

    //Doesn't buffer the entire request body in a StringBuilder unless there aren't any newline characters.
    //    Doesn't call Split on the string.
    //However, there are still a few issues:

    //If newline characters are sparse, much of the request body is buffered in the string.
    //The code continues to create strings (remainingString) and adds them to the string buffer, which results in an extra allocation.
}