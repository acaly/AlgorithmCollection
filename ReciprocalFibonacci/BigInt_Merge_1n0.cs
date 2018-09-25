using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReciprocalFibonacci
{
    class BigInt_Merge_1n0
    {
        public IEnumerable<int> Calculate(int order)
        {
            MultiplyOrder = order;
            Multiply = BigInteger.Pow(10, MultiplyOrder); //can be larger
            Multiply2 = Multiply * Multiply;
            F1 = 1;
            F2 = 2;
            X1 = 3;
            X2 = 4;
            CurrentResult = -1; //2.0 ->3.x
            CurrentEx = 1;
            CurrentTotalEx = 1;
            Entries = new Entry[1000];
            LinkHead = -1;
            EntryCount = EntryEnd = 0;

            List<int> reverser = new List<int>();
            while (true)
            {
                var d = MoveNextDigit();
                if (d.HasValue)
                {
                    var val = d.Value;
                    for (int i = 0; i < MultiplyOrder; ++i)
                    {
                        val = BigInteger.DivRem(val, 10, out var r);
                        reverser.Add((int)r);
                    }
                    for (int i = MultiplyOrder - 1; i >= 0; --i)
                    {
                        yield return reverser[i];
                    }
                    reverser.Clear();
                }
            }
        }

        public int MultiplyOrder;
        public BigInteger Multiply, Multiply2;

        public Entry[] Entries;
        public long LinkHead, EntryEnd;
        public long EntryCount;
        public long NextIndex = 0;
        public Dictionary<long, long> EntryMap = new Dictionary<long, long>();

        public BigInteger CurrentResult;
        public BigInteger CurrentEx;
        public BigInteger CurrentTotalEx;

        public BigInteger F1, F2;
        public BigInteger X1, X2;

        public BigInteger? MoveNextDigit()
        {
            var lastTotalEx = CurrentTotalEx;
            var lastEx = CurrentEx;
            CurrentTotalEx *= Multiply;
            CurrentEx *= Multiply;
            CurrentResult *= Multiply;

            while (F2 < CurrentTotalEx)
            {
                AddEntry(new Entry
                {
                    Denominator = F2,
                    Remainder = lastTotalEx,
                });
                var tmp = F1;
                F1 = F2;
                F2 += tmp;
            }

            for (long i = 0; i < EntryEnd; ++i)
            {
                CurrentResult += StepEntry(i);
            }

            if (CurrentEx - CurrentResult > Multiply2 * (EntryCount + 2))
            {
                var ret = BigInteger.DivRem(CurrentResult, lastEx, out CurrentResult);
                CurrentEx /= Multiply;
                return ret;
            }
            return null;
        }

        public struct Entry
        {
            public BigInteger Denominator;
            public BigInteger Remainder;
            public long LinkedIndex;
        }

        public void AddEntry(Entry e)
        {
            e.LinkedIndex = NextIndex++;
            if ((e.LinkedIndex & 1) == 1 && e.LinkedIndex >= 3)
            {
                var oldIndex = (e.LinkedIndex - 3) / 2;
                var i = EntryMap[oldIndex];
                EntryMap.Remove(oldIndex);

                var div = X2;
                X2 += X1;
                X1 = div;

                Entries[i].Remainder = Entries[i].Remainder * div + e.Remainder;
                Entries[i].Denominator = e.Denominator;
                Entries[i].LinkedIndex = e.LinkedIndex;
                EntryMap[e.LinkedIndex] = i;
                return;
            }
            if (LinkHead != -1)
            {
                var nextHead = Entries[LinkHead].LinkedIndex;
                Entries[LinkHead] = e;
                EntryMap[e.LinkedIndex] = LinkHead;
                LinkHead = nextHead;
            }
            else if (EntryEnd < Entries.Length)
            {
                var newIndex = EntryEnd++;
                Entries[newIndex] = e;
                EntryMap[e.LinkedIndex] = newIndex;
            }
            else
            {
                Entry[] newEntries = new Entry[Entries.Length * 2];
                Array.Copy(Entries, newEntries, Entries.Length);
                Entries = newEntries;
                var newIndex = EntryEnd++;
                Entries[newIndex] = e;
                EntryMap[e.LinkedIndex] = newIndex;
            }
            EntryCount++;
        }

        public void RemoveEntry(long index)
        {
            Entries[index].Denominator = 0;
            Entries[index].LinkedIndex = LinkHead;
            LinkHead = index;
            EntryCount--;
        }

        public BigInteger StepEntry(long index)
        {
            var e = Entries[index];
            if (e.Denominator.IsZero) return 0;
            var ret = BigInteger.DivRem(e.Remainder * Multiply, e.Denominator,
                out Entries[index].Remainder);
            if (Entries[index].Remainder.IsZero)
            {
                RemoveEntry(index);
            }
            return ret;
        }
    }
}
