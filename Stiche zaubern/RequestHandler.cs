namespace Stiche_zaubern
{
    public class RequestHandler
    {
        public bool IsCanceled { get; set; } = true;
        public bool IsSkipable { get; set; } = false;
        public bool SkipRequest { get; set; } = false;
        public bool CancelRequest { get; set; } = false;

        public bool IsToSkip()
        {
            return CancelRequest || (SkipRequest && IsSkipable);
        }
    }
}