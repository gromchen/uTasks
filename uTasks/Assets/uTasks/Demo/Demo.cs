using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace uTasks.Demo
{
    public class Demo : MonoBehaviour
    {
        [SerializeField] private Text _resultText;
        [SerializeField] private InputField _nInputField;
        [SerializeField] private Button _calculateButton;
        [SerializeField] private Button _cancelButton;

        [UsedImplicitly]
        private void Awake()
        {
            TaskScheduler.Current = new UnityTaskScheduler();

            _resultText.text = "Doing nothing";
            _calculateButton.onClick.AddListener(StartTest);
        }

        private void StartTest()
        {
            int n;

            if (int.TryParse(_nInputField.text, out n) == false)
            {
                _resultText.text = "Please enter number";
                return;
            }

            _resultText.text = "Computing";
            TaskFactory.StartNew(() => FindPrimeNumber(n))
                .CompleteWithAction(task => { _resultText.text = string.Format("Completed with {0}", task.Result); });
        }

        private static long FindPrimeNumber(int n)
        {
            var count = 0;
            long a = 2;

            while (count < n)
            {
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