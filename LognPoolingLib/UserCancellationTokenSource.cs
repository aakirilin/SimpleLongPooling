namespace LognPoolingLib
{
    public class UserCancellationTokenSource : CancellationTokenSource
    {
        private readonly string user;

        public UserCancellationTokenSource(string user)
        {
            this.user = user;
        }

        public void Cancel(string user)
        {
            if (user.Equals(this.user))
            {
                base.Cancel();
            }
        }
    }
}
