using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

namespace SortingModule
{
    internal class SortingAlgorithms
    {
        /// <summary>
        /// The main method being called, will take the screen text and call the appropriate method for the algorithm.
        /// </summary>
        /// <param name="algorithm">The string of the algorithm used.</param>
        /// <param name="initialButtons">The button array before any swaps.</param>
        /// <param name="swapButtons">The user's swap.</param>
        /// <param name="totalSwaps">The amount of swaps done previously.</param>
        protected internal bool IfValid(string algorithm, byte[] initialButtons, byte swapButtons, byte totalSwaps, int moduleId, Func<IEnumerable<int>> serialNumber)
        {
            if (algorithm == "SORTED!")
                return true;

            //makes new list so that it doesn't reference the initialbuttons list
            byte[] buttonList = new byte[initialButtons.Length];

            for (int i = 0; i < buttonList.Length; i++)
                buttonList[i] = initialButtons[i];

            Type classType = typeof(SortingAlgorithms);

            //format the text so that it's ready to be called as a method name
            MethodInfo method = classType.GetMethod(FormatMethodName(algorithm));

            //Debug.LogFormat("Running {0}...", FormatMethodName(algorithm));

            //call the method name based on the formatted text, pass the necessary variables through and return whether the swap is valid
            return (bool)method.Invoke(this, new object[] { buttonList, swapButtons, totalSwaps, moduleId, serialNumber });
        }

        /// <summary>
        /// Formats any string to be a method name that's within here. Will not work if the string isn't a member of _algorithms.
        /// </summary>
        /// <param name="algorithm">The string to convert into a method name.</param>
        private string FormatMethodName(string algorithm)
        {
            char[] chars = algorithm.ToCharArray();

            //makes all letters lowercase, besides the first one
            for (int i = 1; i < chars.Length; i++)
                chars[i] = char.ToLower(chars[i]);

            algorithm = new string(chars);

            //Adds "Sort" to the end since every method ends with "Sort".
            algorithm += "Sort";

            //Debug.LogFormat("The return string is: {0}", algorithm);

            return algorithm;
        }

        /// <summary>
        /// Compares the numbers to each other and returns accordingly.
        /// </summary>
        /// <param name="i">Referring to the current instance (for-loop)</param>
        /// <param name="offset">Referring to the offset comparison (distance between indexes)</param>
        /// <param name="buttonList">List containing the buttons, completely unchanged.</param>
        /// <param name="currentSwaps">Stores the amount of swaps done by the algorithm.</param>
        /// <param name="totalSwaps">Takes in the amount of swaps needed before it needs to break.</param>
        /// <param name="swapButtons">The algorithm's answer (First selection is most significant digit, second selection is least significant digit)</param>
        /// <param name="currentButtons">The user's answer (First selection is most significant digit, second selection is least significant digit)</param>
        private bool Compare(ref int i, ref int offset, ref byte[] buttonList, ref byte currentSwaps, ref byte totalSwaps, ref byte currentButtons, ref byte swapButtons, ref int moduleId)
        {
            //if out of range from array, stop before comparing
            if (i + offset >= buttonList.Length || i + offset < 0)
                return false;

            //if the first variable is larger than the second, swap
            if (buttonList[i] > buttonList[i + offset])
            {
                //swapping
                byte temp = buttonList[i];
                buttonList[i] = buttonList[i + offset];
                buttonList[i + offset] = temp;

                currentSwaps++;

                //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                //if the amount of swaps has been met
                if (totalSwaps == currentSwaps)
                {
                    //least significant digit should always be larger quantity
                    if (i < i + offset)
                    {
                        currentButtons = (byte)((i + 1) * 10);
                        currentButtons += (byte)(i + offset + 1);
                    }

                    else
                    {
                        currentButtons = (byte)((i + offset + 1) * 10);
                        currentButtons += (byte)(i + 1);
                    }

                    Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                    //if the answer from the comparison matches the user, it's correct, return true
                    if (swapButtons == currentButtons)
                        return true;

                    return false;
                }
            }

            return false;
        }

        public bool BubbleSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwap = 0, currentButtons = 0;
            int offset = 1;

            for (int i = 0; i < buttonList.Length - 1; i++)
                for (int j = 0; j < buttonList.Length - 1; j++)
                {
                    //Debug.LogFormat("Currently on index {0} (j) in {1} (i).", j, i);

                    //compare the index j with j + 1
                    bool compare = Compare(ref j, ref offset, ref buttonList, ref currentSwap, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }

            return false;
        }

        public bool InsertionSort(byte[] tempButtons, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwap = 0, currentButtons = 0;
            int offset = 1;

            for (int i = 0; i < tempButtons.Length - 1; i++)
            {
                for (int j = i; j >= 0; j--)
                {
                    //Debug.LogFormat("Currently on index {0} (j) in {1} (i).", j, i);

                    //compare the index j with j + 1
                    bool compare = Compare(ref j, ref offset, ref tempButtons, ref currentSwap, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }
            }

            return false;
        }

        public bool CocktailSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            bool swapped;
            byte currentSwaps = 0, currentButtons = 0;
            int offset = 1;

            do
            {
                swapped = false;

                for (int i = 0; i < buttonList.Length - 1; i++)
                {
                    //Debug.LogFormat("Currently on index {0} (i) for the first for-loop.", i);

                    /*
                     * because the compare method will only return true if both the max amount of swaps have been met
                     * and if the user's answer is correct. do a seperate comparison and set the bool to true.
                     * if no swaps occur during this for loop, then we know that the array is sorted.
                    */
                    if (buttonList[i] > buttonList[i + 1])
                        swapped = true;

                    //compare index i with i + 1
                    bool compare = Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }

                if (!swapped)
                    break;

                swapped = false;

                //runs the same calculations as the previous for loop except this one has a backwards for loop
                for (int i = buttonList.Length - 2; i >= 0; i--)
                {
                    //Debug.LogFormat("Currently on index {0} (i) for the second for-loop.", i);

                    if (buttonList[i] > buttonList[i + 1])
                        swapped = true;

                    bool compare = Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }

            } while (swapped);

            return false;
        }

        public bool SelectionSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;

            for (int i = 0; i < buttonList.Length; i++)
            {
                byte smallestValue = buttonList[i], smallestIndex = (byte)i;

                for (int j = i; j < buttonList.Length; j++)
                {
                    if (j == i)
                        continue;

                    //Debug.LogFormat("Currently on index {0} (j) in {1} (i).", j, i);

                    if (buttonList[j] < smallestValue)
                    {
                        smallestValue = buttonList[j];
                        smallestIndex = (byte)j;
                    }
                }

                if (i == smallestIndex)
                    continue;

                //swapping
                byte temp = buttonList[i];
                buttonList[i] = buttonList[smallestIndex];
                buttonList[smallestIndex] = temp;

                currentSwaps++;

                //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                if (totalSwaps == currentSwaps)
                {
                    currentButtons = (byte)((i + 1) * 10);
                    currentButtons += (byte)(smallestIndex + 1);

                    Debug.LogFormat("[Sorting #{0}] Expecting label swaps {1}. Recieved label swaps {2}.", moduleId, currentButtons, swapButtons);

                    if (swapButtons == currentButtons)
                        return true;

                    return false;
                }
            }

            return false;
        }

        public bool RadixSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0, smallestIndex = 0;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < buttonList.Length; j++)
                {
                    if (buttonList[j] % 10 == i)
                    {
                        if (buttonList[j] == buttonList[smallestIndex])
                        {
                            smallestIndex++;
                            continue;
                        }

                        //swapping
                        byte temp = buttonList[j];
                        buttonList[j] = buttonList[smallestIndex];
                        buttonList[smallestIndex] = temp;

                        currentSwaps++;

                        //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                        if (totalSwaps == currentSwaps)
                        {
                            currentButtons = (byte)((smallestIndex + 1) * 10);
                            currentButtons += (byte)(j + 1);

                            Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                            if (swapButtons == currentButtons)
                                return true;

                            return false;
                        }

                        smallestIndex++;
                    }
                }

                if (smallestIndex == 4)
                    break;
            }

            smallestIndex = 0;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < buttonList.Length; j++)
                {
                    if (buttonList[j] / 10 == i)
                    {
                        if (j == smallestIndex)
                        {
                            smallestIndex++;
                            continue;
                        }

                        //Debug.LogFormat("Smallest Index: {0}, J: {1}, buttonList / 10: {2}", smallestIndex, j, buttonList[j] / 10);

                        //swapping
                        byte temp = buttonList[j];
                        buttonList[j] = buttonList[smallestIndex];
                        buttonList[smallestIndex] = temp;

                        currentSwaps++;

                        //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                        if (totalSwaps == currentSwaps)
                        {
                            currentButtons = (byte)((smallestIndex + 1) * 10);
                            currentButtons += (byte)(j + 1);

                            Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                            if (swapButtons == currentButtons)
                                return true;

                            return false;
                        }

                        smallestIndex++;
                    }
                }

                if (smallestIndex == 4)
                    break;
            }

            if (totalSwaps != 250)
                return true;

            return false;
        }

        public bool MergeSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            bool compare = false;
            byte currentSwaps = 0, currentButtons = 0;
            int offset = 1;

            //swap 1 & 2, swap 4 & 5
            for (int i = 0; i < buttonList.Length; i += 3)
            {
                compare = Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                if (compare)
                    return true;
            }

            //determine which side 3 should merge with
            if (serialNumber().First() % 2 == 1)
            {
                for (int i = 0; i <= 1; i++)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        //Debug.LogFormat("j: {0}", j);
                        if (i == j)
                            continue;

                        byte biggerThan = 0;

                        for (int k = 0; k <= 2; k++)
                        {
                            if (buttonList[j] > buttonList[k])
                                biggerThan++;
                        }

                        //Debug.LogFormat("i: {0}, j: {1}, biggerThan: {2}", i, j, biggerThan);
                        if (biggerThan == i)
                        {
                            //swapping
                            byte temp = buttonList[j];
                            buttonList[j] = buttonList[i];
                            buttonList[i] = temp;

                            currentSwaps++;

                            //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                            if (totalSwaps == currentSwaps)
                            {
                                currentButtons = (byte)((i + 1) * 10);
                                currentButtons += (byte)(j + 1);

                                Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                                if (swapButtons == currentButtons)
                                    return true;

                                return false;
                            }
                        }
                    }
                }
            }

            else
            {
                for (int i = 4; i >= 3; i--)
                {
                    for (int j = 4; j >= 2; j--)
                    {
                        if (i == j)
                            continue;

                        byte biggerThan = 0;

                        for (int k = 4; k >= 2; k--)
                        {
                            if (buttonList[j] > buttonList[k])
                                biggerThan++;
                        }

                        //Debug.LogFormat("i: {0}, j: {1}, biggerThan: {2}", i - 2, j, biggerThan);
                        if (biggerThan == i - 2)
                        {
                            //swapping
                            byte temp = buttonList[j];
                            buttonList[j] = buttonList[i];
                            buttonList[i] = temp;

                            currentSwaps++;

                            //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                            if (totalSwaps == currentSwaps)
                            {
                                currentButtons = (byte)((j + 1) * 10);
                                currentButtons += (byte)(i + 1);

                                Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                                if (swapButtons == currentButtons)
                                    return true;

                                return false;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < buttonList.Length; i++)
            {
                byte smallestValue = buttonList[i], smallestIndex = (byte)i;

                for (int j = i; j < buttonList.Length; j++)
                {
                    if (j == i)
                        continue;

                    //Debug.LogFormat("Currently on index {0} (j) in {1} (i).", j, i);

                    if (buttonList[j] < smallestValue)
                    {
                        smallestValue = buttonList[j];
                        smallestIndex = (byte)j;
                    }
                }

                if (i == smallestIndex)
                    continue;

                //swapping
                byte temp = buttonList[i];
                buttonList[i] = buttonList[smallestIndex];
                buttonList[smallestIndex] = temp;

                currentSwaps++;

                //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                if (totalSwaps == currentSwaps)
                {
                    currentButtons = (byte)((i + 1) * 10);
                    currentButtons += (byte)(smallestIndex + 1);

                    Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                    if (swapButtons == currentButtons)
                        return true;

                    return false;
                }
            }

            return false;
        }

        public bool CombSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;

            for (int i = 3; i > 0; i--)
            {
                for (int j = 0; j < buttonList.Length; j++)
                {
                    bool compare = Compare(ref j, ref i, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }
            }

            return false;
        }

        public bool OddevenSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;
            int offset = 1;

            for (int i = 0; i < buttonList.Length; i++)
            {
                for (int j = 0; j < buttonList.Length; j += 2)
                {
                    bool compare = Compare(ref j, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }

                for (int j = 1; j < buttonList.Length; j += 2)
                {
                    bool compare = Compare(ref j, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }
            }

            return false;
        }

        public bool HeapSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;
            int index = 0, offset = 0;

            for (int i = 1; i <= 15; i++)
            {
                switch (i)
                {
                    //4th > 2nd
                    case 1:
                    case 5:
                    case 10:
                        index = 3;
                        offset = -2;
                        break;

                    //5th > 2nd
                    case 2:
                    case 6:
                        index = 4;
                        offset = -3;
                        break;

                    //2nd > 1st
                    case 3:
                    case 8:
                    case 12:
                        index = 1;
                        offset = -1;
                        break;

                    //3rd > 1st
                    case 4:
                    case 9:
                    case 13:
                        index = 2;
                        offset = -2;
                        break;

                    //1st > 5th
                    case 7:
                        index = 0;
                        offset = 4;
                        break;

                    //1st > 4th
                    case 11:
                        index = 0;
                        offset = 3;
                        break;

                    //1st > 3rd
                    case 14:
                        index = 0;
                        offset = 2;
                        break;

                    //1st > 2nd
                    case 15:
                        index = 0;
                        offset = 1;
                        break;
                }

                //Debug.LogFormat("{0}", i);

                bool compare = Compare(ref index, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                if (compare)
                    return true;
            }

            return false;
        }

        public bool CycleSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;
            int index = 0;

            while (true)
            {
                //Debug.LogFormat("Index is currently: {0}", index);
                byte biggerThan = 0;

                for (int i = 0; i < buttonList.Length; i++)
                {
                    if (buttonList[index] > buttonList[i])
                        biggerThan++;
                }

                if (biggerThan == index)
                {
                    index++;
                    continue;
                }

                //swapping
                byte temp = buttonList[index];
                buttonList[index] = buttonList[biggerThan];
                buttonList[biggerThan] = temp;

                currentSwaps++;

                //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                if (totalSwaps == currentSwaps)
                {
                    //least significant digit should always be larger quantity
                    if (index < biggerThan)
                    {
                        currentButtons = (byte)((index + 1) * 10);
                        currentButtons += (byte)(biggerThan + 1);
                    }

                    else
                    {
                        currentButtons = (byte)((biggerThan + 1) * 10);
                        currentButtons += (byte)(index + 1);
                    }

                    Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                    if (swapButtons == currentButtons)
                        return true;

                    break;
                }
            }

            return false;
        }

        public bool FiveSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;
            int middleValue = 2;

            //get median value
            for (int i = 0; i < buttonList.Length; i++)
            {
                byte biggerThan = 0;

                for (int j = 0; j < buttonList.Length; j++)
                {
                    if (buttonList[i] > buttonList[j])
                        biggerThan++;
                }

                if (biggerThan == 2)
                {
                    middleValue = (byte)i;
                    break;
                }
            }

            int offset = 2 - middleValue;

            //swap median with 3rd
            if (offset != 0)
            {
                //swapping
                byte temp = buttonList[2];
                buttonList[2] = buttonList[middleValue];
                buttonList[middleValue] = temp;

                currentSwaps++;
                offset++;

                //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                if (totalSwaps == currentSwaps)
                {
                    //least significant digit should always be larger quantity
                    if (2 < middleValue)
                    {
                        currentButtons = 30;
                        currentButtons += (byte)(middleValue + 1);
                    }

                    else
                    {
                        currentButtons = (byte)((middleValue + 1) * 10);
                        currentButtons += 3;
                    }

                    Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                    if (swapButtons == currentButtons)
                        return true;
                }
            }

            offset = 1;
            middleValue = 2;

            //put appropriate labels in the two halves
            for (int i = 0; i < 2; i++)
            {
                //Debug.LogFormat("second stage {0} - {1}", i, buttonList[i] > buttonList[middleValue]);
                if (buttonList[i] > buttonList[2])
                {
                    if (buttonList[3] > buttonList[2])
                        offset++;

                    //swapping
                    byte temp = buttonList[i];
                    buttonList[i] = buttonList[2 + offset];
                    buttonList[2 + offset] = temp;

                    currentSwaps++;

                    //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                    if (totalSwaps == currentSwaps)
                    {
                        //least significant digit should always be larger quantity
                        currentButtons = (byte)((i + 1) * 10);
                        currentButtons += (byte)(middleValue + offset + 1);

                        Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                        if (swapButtons == currentButtons)
                            return true;

                        break;
                    }
                }
            }

            //sort halves
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < buttonList.Length; j += 3)
                {
                    //Debug.LogFormat("third stage {0} - {1}", i, buttonList[i] > buttonList[i + 1]);

                    //Debug.LogFormat("true");

                    if (buttonList[j] > buttonList[j + 1])
                    {
                        //swapping
                        byte temp = buttonList[j];
                        buttonList[j] = buttonList[j + 1];
                        buttonList[j + 1] = temp;

                        currentSwaps++;

                        //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                        if (totalSwaps == currentSwaps)
                        {
                            //least significant digit should always be larger quantity
                            if (j < j + 1)
                            {
                                currentButtons = (byte)((j + 1) * 10);
                                currentButtons += (byte)(j + 1 + 1);
                            }

                            else
                            {
                                currentButtons = (byte)((j + 1 + 1) * 10);
                                currentButtons += (byte)(j + 1);
                            }

                            Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                            if (swapButtons == currentButtons)
                                return true;

                            return false;
                        }
                    }
                }
            }

            return false;
        }

        public bool QuickSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            bool compare = false;
            byte currentSwaps = 0, currentButtons = 0;
            int pivot = 0, current = buttonList.Length - 1;

            for (int i = 0; i < buttonList.Length * 10; i++)
            {
                //Debug.LogFormat("iteration: {0}, pivot: {1}, current: {2}", i, pivot, current);

                //if the smaller is to the right or the bigger number to the left relative to pivot, swap
                if ((buttonList[pivot] > buttonList[current] && pivot < current) || (buttonList[pivot] < buttonList[current] && pivot > current))
                {
                    //Debug.LogFormat("above is true");

                    //swaps the labels
                    int temp = pivot;
                    pivot = current;
                    current = temp;

                    //swaps the buttons
                    temp = buttonList[pivot];
                    buttonList[pivot] = buttonList[current];
                    buttonList[current] = (byte)temp;

                    currentSwaps++;

                    //Debug.LogFormat("Swapped {0} times.", currentSwaps);

                    //if the amount of swaps has been met
                    if (totalSwaps == currentSwaps)
                    {
                        //least significant digit should always be larger quantity
                        if (pivot < current)
                        {
                            currentButtons = (byte)((pivot + 1) * 10);
                            currentButtons += (byte)(current + 1);
                        }

                        else
                        {
                            currentButtons = (byte)((current + 1) * 10);
                            currentButtons += (byte)(pivot + 1);
                        }

                        Debug.LogFormat("[Sorting #{0}] Swap Number {1}. Expecting position swaps {2} and {3}. Received position swaps {4} and {5}.", moduleId, totalSwaps, currentButtons / 10, currentButtons % 10, swapButtons / 10, swapButtons % 10);

                        //if the answer from the comparison matches the user, it's correct, return true
                        if (swapButtons == currentButtons)
                            return true;

                        return false;
                    }

                    if (compare)
                        return true;
                }

                if (pivot > current)
                    current++;

                else
                    current--;

                //Debug.LogFormat("iteration: {0}, pivot: {1}, current: {2}", i, pivot, current);

                //if both are the same, repeat this but with new indexes for pivot and current
                if (pivot == current)
                {
                    List<byte> sorted = new List<byte>(0);

                    //determines how many indexes are bigger than each other
                    for (int j = 0; j < buttonList.Length; j++)
                    {
                        //Debug.LogFormat("index in biggerThan for loop: {0}", j);

                        byte biggerThan = 0;

                        //calculates amount of indexes bigger than j
                        for (int k = 0; k < buttonList.Length; k++)
                        {
                            if (buttonList[j] > buttonList[k])
                                biggerThan++;
                        }

                        //Debug.LogFormat("biggerThan variable: {0}", biggerThan);

                        //add it to the list if they match their position since it's sorted
                        if (j == biggerThan)
                        {
                            sorted.Add((byte)j);
                            //Debug.LogFormat("added {0} to list", j);
                        }
                    }

                    //find the first elements that isn't already sorted
                    for (int j = 0; j < buttonList.Length; j++)
                    {
                        if (!sorted.Contains((byte)j))
                        {
                            pivot = j;
                            break;
                        }
                    }

                    //find the first elements that isn't already sorted
                    for (int j = buttonList.Length - 1; j >= 0; j--)
                    {
                        if (!sorted.Contains((byte)j))
                        {
                            current = j;
                            break;
                        }
                    }

                    //Debug.LogFormat("Pivot: {0}. Current: {1}.", pivot, current);

                    //for (int k = 0; k < sorted.Count; k++)
                    //Debug.LogFormat("Sorted: {0}", sorted[k]);
                }
            }

            return false;
        }

        public bool SlowSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;
            int index = 0, offset = 0;

            for (int i = 1; i <= 11; i++)
            {
                switch (i)
                {
                    //1st > 2nd
                    case 1:
                    case 3:
                    case 6:
                    case 9:
                    case 11:
                        index = 0;
                        offset = 1;
                        break;

                    //2nd > 3rd
                    case 2:
                    case 10:
                        index = 1;
                        offset = 1;
                        break;

                    //4th > 5th
                    case 4:
                        index = 3;
                        offset = 1;
                        break;

                    //3rd > 5th
                    case 5:
                        index = 2;
                        offset = 2;
                        break;

                    //3rd > 4th
                    case 7:
                        index = 2;
                        offset = 1;
                        break;

                    //2nd > 4th
                    case 8:
                        index = 1;
                        offset = 2;
                        break;
                }

                //Debug.LogFormat("{0}", i);

                bool compare = Compare(ref index, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                if (compare)
                    return true;
            }

            return false;
        }

        public bool ShellSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;

            bool compare;
            int j;
            for (int i = 2; i > 0; i --)
            {
                for (j = 0; j < buttonList.Length; j++)
                {
                    compare = Compare(ref j, ref i, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }

                j = 0;
                compare = Compare(ref j, ref i, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                if (compare)
                    return true;
            }

            for (int i = 0; i < 2; i++)
            {
                for (j = 0; j < buttonList.Length; j++)
                {
                    int offset = 1;
                    compare = Compare(ref j, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId);

                    if (compare)
                        return true;
                }
            }

            return false;
        }

        public bool StoogeSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            byte currentSwaps = 0, currentButtons = 0;

            if (StoogeSortSubArray(0, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(1, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(0, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;

            if (StoogeSortSubArray(1, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(2, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(1, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;

            if (StoogeSortSubArray(0, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(1, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;
            if (StoogeSortSubArray(0, buttonList, swapButtons, totalSwaps, moduleId, ref serialNumber, ref currentSwaps, ref currentButtons))
                return true;

            return false;
        }

        private bool StoogeSortSubArray(byte n, byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber, ref byte currentSwaps, ref byte currentButtons)
        {
            int offset = 1;

            int i = n;
            if (Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId))
                return true;

            i = (byte)(n + 1);
            if (Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId))
                return true;

            i = n;
            if (Compare(ref i, ref offset, ref buttonList, ref currentSwaps, ref totalSwaps, ref currentButtons, ref swapButtons, ref moduleId))
                return true;

            return false;
        }

        public bool BogoSort(byte[] buttonList, byte swapButtons, byte totalSwaps, int moduleId, ref Func<IEnumerable<int>> serialNumber)
        {
            return totalSwaps != 250;
        }
    }
}