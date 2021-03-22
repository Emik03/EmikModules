using System;

namespace Linq
{
    internal static class LinqValidate
    {
        internal static bool[] Run(string serial, bool[] initButtons, LinqFunctions function, object parameter)
        {
            bool[] answer = new bool[6];
            Array.Copy(initButtons, answer, 6);

            switch (function)
            {
                case LinqFunctions.First:
                    int first = Array.IndexOf(answer, true);
                    answer = new bool[6];
                    answer[first] = true;
                    break;

                case LinqFunctions.Last:
                    int last = Array.LastIndexOf(answer, true);
                    answer = new bool[6];
                    answer[last] = true;
                    break;

                case LinqFunctions.Min:
                    int min = 'Z';
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (!answer[i])
                            continue;
                        if (min > serial[i])
                            min = serial[i];
                    }
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (!answer[i])
                            continue;
                        if (min != serial[i])
                            answer[i] = false;
                    }
                    break;

                case LinqFunctions.Max:
                    int max = '0';
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (!answer[i])
                            continue;
                        if (max < serial[i])
                            max = serial[i];
                    }
                    for (int i = 0; i < answer.Length; i++)
                    {
                        if (!answer[i])
                            continue;
                        if (max != serial[i])
                            answer[i] = false;
                    }
                    break;

                case LinqFunctions.Distinct:
                    string currentSearch = string.Empty;
                    for (int i = 0; i < answer.Length; i++)
                        if (currentSearch.Contains(serial[i].ToString()))
                            answer[i] = false;
                        else
                            currentSearch += serial[i];
                    break;

                case LinqFunctions.Skip:
                    for (int i = 0; i < answer.Length && (int)parameter > 0; i++)
                        if (answer[i])
                        {
                            answer[i] = false;
                            parameter = (int)parameter - 1;
                        }
                    break;

                case LinqFunctions.SkipLast:
                    for (int i = answer.Length - 1; i >= 0 && (int)parameter > 0; i--)
                        if (answer[i])
                        {
                            answer[i] = false;
                            parameter = (int)parameter - 1;
                        }
                    break;

                case LinqFunctions.Take:
                    for (int i = 0; i < answer.Length; i++)
                        if (answer[i])
                        {
                            if ((int)parameter <= 0)
                                answer[i] = false;
                            parameter = (int)parameter - 1;
                        }
                    break;

                case LinqFunctions.TakeLast:
                    for (int i = answer.Length - 1; i >= 0; i--)
                        if (answer[i])
                        {
                            if ((int)parameter <= 0)
                                answer[i] = false;
                            parameter = (int)parameter - 1;
                        }
                    break;

                case LinqFunctions.ElementAt:
                    int at = -1;
                    for (int i = 0; i < answer.Length; i++)
                        if (answer[i])
                        {
                            if ((int)parameter <= 0)
                            {
                                at = i;
                                break;
                            }
                            parameter = (int)parameter - 1;
                        }
                    answer = new bool[6];
                    answer[at] = true;
                    break;

                case LinqFunctions.Except:
                    for (int i = 0; i < answer.Length; i++)
                        if (answer[i] && parameter.ToString().Contains(serial[i].ToString()))
                            answer[i] = false;
                    break;
                case LinqFunctions.Intersect:
                    for (int i = 0; i < answer.Length; i++)
                        if (answer[i] && !parameter.ToString().Contains(serial[i].ToString()))
                            answer[i] = false;
                    break;

                case LinqFunctions.Concat:
                    for (int i = 0; i < answer.Length; i++)
                        if (parameter.ToString().Contains(serial[i].ToString()))
                            answer[i] = true;
                    break;

                case LinqFunctions.Append:
                    int lastLit;
                    for (lastLit = answer.Length - 1; lastLit >= 0; lastLit--)
                        if (answer[lastLit])
                            break;
                    for (int i = lastLit; i < answer.Length; i++)
                        if (parameter.ToString().Contains(serial[i].ToString()))
                            answer[i] = true;
                    break;

                case LinqFunctions.Prepend:
                    int firstLit;
                    for (firstLit = 0; firstLit < answer.Length; firstLit++)
                        if (answer[firstLit])
                            break;
                    for (int i = 0; i < firstLit; i++)
                        if (parameter.ToString().Contains(serial[i].ToString()))
                            answer[i] = true;
                    break;

                default: throw new NotImplementedException();
            }

            return answer;
        }
    }
}
