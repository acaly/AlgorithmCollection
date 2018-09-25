using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReciprocalFibonacci
{
    class BigInt_100
    {
        public IEnumerable<int> Calculate(int mul)
        {
            if (mul > 16) throw new ArgumentOutOfRangeException(nameof(mul));
            long mulnum = (long)Math.Pow(10, mul);
            Multiply = mulnum;
            Multiply2 = Multiply * Multiply;
            F1 = 1;
            F2 = 2;
            CurrentResult = -1; //2.0 ->3.x
            CurrentEx = 1;
            CurrentTotalEx = 1;
            Entries = new Entry[1000];
            LinkHead = -1;
            EntryCount = EntryEnd = 0;
            while (true)
            {
                var d = MoveNextDigit();
                if (d.HasValue)
                {
                    var digit = mulnum / 10;
                    var val = d.Value;
                    while (digit > 0)
                    {
                        var ret = (int)(val / digit);
                        val = val - ret * digit;
                        digit /= 10;
                        yield return ret;
                    }
                }
            }
        }

        public BigInteger Multiply, Multiply2;

        public Entry[] Entries;
        public long LinkHead, EntryEnd;
        public long EntryCount;

        public BigInteger CurrentResult;
        public BigInteger CurrentEx;
        public BigInteger CurrentTotalEx;

        public BigInteger F1, F2;

        public long? MoveNextDigit()
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
                return (long)ret;
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
            if (LinkHead != -1)
            {
                var nextHead = Entries[LinkHead].LinkedIndex;
                Entries[LinkHead] = e;
                LinkHead = nextHead;
            }
            else if (EntryEnd < Entries.Length)
            {
                Entries[EntryEnd++] = e;
            }
            else
            {
                Entry[] newEntries = new Entry[Entries.Length * 2];
                Array.Copy(Entries, newEntries, Entries.Length);
                Entries = newEntries;
                Entries[EntryEnd++] = e;
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

        public long StepEntry(long index)
        {
            var e = Entries[index];
            if (e.Denominator.IsZero) return 0;
            var ret = (long)BigInteger.DivRem(e.Remainder * Multiply, e.Denominator,
                out Entries[index].Remainder);
            if (Entries[index].Remainder.IsZero)
            {
                RemoveEntry(index);
            }
            return ret;
        }
    }
}
