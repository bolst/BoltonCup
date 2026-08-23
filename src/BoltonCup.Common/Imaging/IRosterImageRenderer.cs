namespace BoltonCup.Common.Imaging;

public interface IRosterImageRenderer
{
    /// <summary>
    /// Renders the roster card as PNG bytes. The base layout is 1080x1220; <paramref name="scale"/>
    /// supersamples the output (e.g. 3 = 3240x3660) for a crisp export. Use 1 for cheap previews.
    /// </summary>
    byte[] Render(RosterImageModel model, float scale = 1f);
}