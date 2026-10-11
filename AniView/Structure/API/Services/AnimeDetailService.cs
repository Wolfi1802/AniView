using AniView.Models;
using AniView.Structure.API.Dtos;
using AniView.Structure.API.Exceptions;
using AniView.Structure.API.Mapping;
using AniView.Structure.API.Querys;
using Microsoft.Extensions.Caching.Memory;

namespace AniView.Structure.API.Services
{
    public sealed class AnimeDetailService : IAnimeDetailService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        // Die Detailansicht zeigt alle Objekte der zweiten Ebene.
        private static readonly MediaFieldsProfile Profile = MediaFieldsProfile.Full;

        private readonly IAniListClient _client;
        private readonly IMemoryCache _cache;

        public AnimeDetailService(IAniListClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task<AnimeDetails> GetDetailsAsync(int animeId, CancellationToken cancellationToken = default)
        {
            if (animeId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(animeId), "Die Anime-ID muss größer als 0 sein.");
            }

            var cacheKey = $"anilist:detail:{animeId}:{Profile.CacheKey}";

            if (_cache.TryGetValue(cacheKey, out AnimeDetails? cached) && cached is not null)
            {
                return cached;
            }

            var data = await _client.SendAsync<MediaByIdData>(
                ApiQueries.MediaById(Profile),
                new { id = animeId },
                cancellationToken);

            if (data.Media is null)
            {
                throw new ApiException("Zu diesem Anime wurden keine Informationen gefunden.");
            }

            var details = AnimeDetailsMapper.Map(data.Media);

            _cache.Set(cacheKey, details, CacheDuration);

            return details;
        }
    }
}
