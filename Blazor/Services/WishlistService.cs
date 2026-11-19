namespace Blazor.Services
{
    // This project seeks to demonstrate wishlist logic without setting up a persistent database.
    // Since this Blazor project uses SignalR, we cannot use httpcontext.session without complicating things.
    // Therefore we create this wishlistService instead as a per user session storage.
    public class WishlistService
    {
        private readonly List<int> movieIds = [];

        public event Action? Changed;

        public IReadOnlyList<int> MovieIds => movieIds;

        public bool Contains(int movieId) => movieIds.Contains(movieId);

        public void Toggle(int movieId)
        {
            if (!movieIds.Remove(movieId))
                movieIds.Add(movieId);

            Changed?.Invoke();
        }

        public void Remove(int movieId)
        {
            if (movieIds.Remove(movieId))
                Changed?.Invoke();
        }
    }
}