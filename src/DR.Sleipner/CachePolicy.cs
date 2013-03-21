namespace DR.Sleipner
{
    public class CachePolicy
    {
        public int CacheDuration;
        public int MaxAge;
        public int ExceptionCacheDuration = 10;
        public bool BubbleExceptions;
        public string CachePool = "";
    }
}