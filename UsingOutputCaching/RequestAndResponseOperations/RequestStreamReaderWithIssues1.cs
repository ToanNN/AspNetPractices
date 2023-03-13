using System.Buffers;
using System.Text;

namespace UsingOutputCaching.RequestAndResponseOperations;

//Suppose the goal is to create a middleware that reads the entire request body as a list of strings, splitting on new lines.
public class RequestStreamReaderWithIssues1
{
    internal async Task<List<string>> GetStringsFromStreamWithIssues1(Stream body)
    {
        var builder = new StringBuilder();

        //Rent a shared buffer to write the request body to
        byte[] buffer = ArrayPool<byte>.Shared.Rent(4096);
        while (true)
        {
            var bytesRead = await body.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }

            // append encoded string into the string builder
            var encodedString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            builder.Append(encodedString);
        }

        ArrayPool<byte>.Shared.Return(buffer);

        var entireRequestBody = builder.ToString();

        // Split on \n in the string.
        return new List<string>(entireRequestBody.Split("\n"));
    }

    //This code works, but there are some issues:

    //Before appending to the StringBuilder, the example creates another string (encodedString) that is thrown away immediately.This process occurs for all bytes in the stream, so the result is extra memory allocation the size of the entire request body.
    //    The example reads the entire string before splitting on new lines.It's more efficient to check for new lines in the byte array.
}