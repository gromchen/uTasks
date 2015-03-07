namespace uTasks
{
    public struct CancellationToken
    {
        private CancellationTokenSource _source;

        public CancellationToken(CancellationTokenSource source)
        {
            _source = source;
        }
    }
}