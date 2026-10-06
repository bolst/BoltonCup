using BoltonCup.Core;

namespace BoltonCup.WebAPI.Mapping;

#pragma warning disable CS1591 // Disable warning for missing XML comments

public partial class Mapper
{
    // ---------- Album ----------

    public GetAlbumsQuery ToQuery(GetAlbumsRequest request) => new GetAlbumsQuery
    {
        Label = request.Label,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<AlbumDto> ToDtoList(IPagedList<Album> albums) => albums.ProjectTo(album => new AlbumDto
    {
        Id = album.Id,
        Title = album.Title,
        Slug = album.Slug,
        CoverImageUrl = _urlResolver.GetFullUrl(album.CoverImage?.Key ?? album.Images.FirstOrDefault()?.Key),
        OccurredAt = album.OccurredAt,
        Tags = ToTagDtos(album.Tags),
    });

    public AlbumSingleDto? ToDto(Album? album) => album is null
            ? null
            : new AlbumSingleDto
            {
                Id = album.Id,
                Title = album.Title,
                Slug = album.Slug,
                CoverImageUrl = _urlResolver.GetFullUrl(album.CoverImage?.Key ?? album.Images.FirstOrDefault()?.Key),
                OccurredAt = album.OccurredAt,
                Tags = ToTagDtos(album.Tags),
                Description = album.Description,
                Source = album.Source,
                Images = album.Images.Select(ToDto).ToList(),
            };

    AlbumImageDto ToDto(AlbumImage image) => new AlbumImageDto
    {
        Id = image.Id,
        Url = _urlResolver.GetFullUrl(image.Key) ?? string.Empty,
        Tags = ToTagDtos(image.Tags),
    };

    public GetAlbumImagesQuery ToQuery(GetAlbumImagesRequest request) => new GetAlbumImagesQuery
    {
        TagType = request.TagType,
        TargetId = request.TagTargetId,
        Page = request.Page,
        Size = request.Size,
        SortBy = request.SortBy,
        Descending = request.Descending,
    };

    public IPagedList<AlbumImageDto> ToDtoList(IPagedList<AlbumImage> images) => images.ProjectTo(ToDto);
}