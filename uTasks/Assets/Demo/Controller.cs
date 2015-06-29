using JetBrains.Annotations;
using uTasks;
using uTasks.Dispatchers;
using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class Controller : MonoBehaviour
    {
        #region Editor

        [SerializeField] private Text _resultText;
        [SerializeField] private InputField _nInputField;
        [SerializeField] private Button _calculateButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private UnityThreadDispatcher _dispatcher;

        #endregion

        private CancellationTokenSource _tokenSource;
        
        [UsedImplicitly]
        private void Awake()
        {
            _resultText.text = "Doing nothing";
            _calculateButton.onClick.AddListener(StartCalculation);
            _cancelButton.onClick.AddListener(CancelCalculation);
        }

        private void StartCalculation()
        {
            int n;

            if (int.TryParse(_nInputField.text, out n) == false)
            {
                _resultText.text = "Please enter number";
                return;
            }

            _tokenSource = new CancellationTokenSource();
            _resultText.text = "Computing";
            Task.Run(() => FindPrimeNumber(n, _tokenSource.Token))
                .ThenInvoke(_dispatcher, task =>
                {
                    if (task.IsCanceled)
                    {
                        _resultText.text = "Task was canceled";
                        return;
                    }

                    _resultText.text = string.Format("Completed with {0}", task.Result);
                });
        }

        private void CancelCalculation()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }
        }

        private static long FindPrimeNumber(int n, CancellationToken token)
        {
            var count = 0;
            long a = 2;

            while (count < n)
            {
                token.ThrowIfCancellationRequested();

                long b = 2;
                var prime = 1; // to check if found a prime

                while (b*b <= a)
                {
                    if (a%b == 0)
                    {
                        prime = 0;
                        break;
                    }

                    b++;
                }

                if (prime > 0)
                    count++;

                a++;
            }

            return (--a);
        }
    }
}