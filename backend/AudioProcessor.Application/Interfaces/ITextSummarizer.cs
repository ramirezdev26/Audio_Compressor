namespace AudioProcessor.Application.Interfaces;

public interface ITextSummarizer
{
    Task<string> SummarizeAsync(string text, int maxLength);
}
