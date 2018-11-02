using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitWord
{
    class SimpleBacktracking
    {
        private SearchNode[] _root = new SearchNode[1];
        private List<WordInfo> _words = new List<WordInfo>();

        public SimpleBacktracking(IEnumerable<string> dict)
        {
            var a = dict.Distinct().OrderBy(s => s.Length);
            _words.Add(new WordInfo());
            int i = 0;
            foreach (var str in a)
            {
                _words.Add(new WordInfo
                {
                    Word = str,
                    BackIndex = FindPrefix(str),
                });
                _root[0].Put(str, 0, _words.Count - 1);
            }
        }

        private int FindPrefix(string str)
        {
            SearchNode[] nodes = _root;
            int index = 0;
            int lastIndex = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                nodes[index].Find(str[i], out nodes, out index);
                if (nodes == null)
                {
                    return lastIndex;
                }
                lastIndex = Math.Max(lastIndex, nodes[index].WordIndex);
            }
            return lastIndex;
        }

        private string _str;
        private int _strPos;
        private List<int> _results;
        private Stack<int> _backtrackRCount;
        private SearchNode[] _currentNodeList;
        private int _currentNodeIndex;
        private int _lastResult;
        private HashSet<int> _invalidPos;
        private int _peekDist;

        private void Restart()
        {
            _currentNodeList = _root;
            _currentNodeIndex = 0;
            _lastResult = 0;
            _peekDist = 0;
        }

        private void AppendResult(int r)
        {
            var bi = _words[r].BackIndex;
            if (bi != 0)
            {
                _backtrackRCount.Push(_results.Count);
            }
            _results.Add(r);
        }

        private void Backtrack()
        {
            var b = _backtrackRCount.Pop();
            for (int i = _results.Count - 1; i >= b; --i)
            {
                _invalidPos.Add(_strPos);
                _strPos -= _words[_results[i]].Word.Length;
            }

            var replace = _words[_results[b]].BackIndex;
            _strPos += _words[replace].Word.Length;
            _results.RemoveRange(b, _results.Count - b);
            AppendResult(replace);
            Restart();
        }

        private bool TryBacktrack()
        {
            //At the end but not found. Try go back
            if (_backtrackRCount.Count == 0)
            {
                return false;
            }
            Backtrack();
            return true;
        }

        private bool TryLastResult()
        {
            if (_lastResult != 0)
            {
                _strPos += _words[_lastResult].Word.Length;
                AppendResult(_lastResult);
                Restart();
                return true;
            }
            return false;
        }

        public List<string> Calculate(string str)
        {
            _str = str;
            _strPos = 0;
            _results = new List<int>();
            _backtrackRCount = new Stack<int>();
            _invalidPos = new HashSet<int>();

            _currentNodeList = _root;
            _currentNodeIndex = 0;
            
            _lastResult = 0;

            _peekDist = 0;

            while (true)
            {
                if (_currentNodeList == _root)
                {
                    if (_invalidPos.Contains(_strPos))
                    {
                        if (!TryBacktrack())
                        {
                            return null;
                        }
                    }
                }
                if (_strPos == str.Length)
                {
                    var current = _currentNodeList[_currentNodeIndex].WordIndex;
                    if (current != 0)
                    {
                        _results.Add(current);
                        return _results.Select(i => _words[i].Word).ToList();
                    }
                }
                else
                {
                    var current = _currentNodeList[_currentNodeIndex].WordIndex;
                    _currentNodeList[_currentNodeIndex].Find(str[_strPos], out _currentNodeList,
                        out _currentNodeIndex);

                    if (current != 0)
                    {
                        _lastResult = current;
                    }

                    if (_currentNodeList != null)
                    {
                        _strPos++;
                        _peekDist++;
                        continue;
                    }

                    if (current != 0)
                    {
                        AppendResult(current);
                        Restart();
                        continue;
                    }
                }

                _strPos -= _peekDist;
                if (TryLastResult() || TryBacktrack())
                {
                    continue;
                }
                return null;
            }
        }
        
        private struct WordInfo
        {
            public string Word;
            public int BackIndex;
        }

        private struct SearchNode
        {
            public int WordIndex;
            public int SearchTableStart;
            public int SearchTableEnd;
            public SearchNode[] Next;

            //public string DebugInfo;

            public void Find(char ch, out SearchNode[] l, out int index)
            {
                if (Next == null || ch < SearchTableStart || ch >= SearchTableEnd)
                {
                    l = null;
                    index = 0;
                }
                else
                {
                    l = Next;
                    index = ch - SearchTableStart;
                }
            }

            public void Put(string word, int level, int wordIndex)
            {
                if (Next == null)
                {
                    Next = new SearchNode[1];
                    SearchTableStart = word[level];
                    SearchTableEnd = word[level] + 1;
                    //Next[0].DebugInfo = word.Substring(0, level + 1);
                    if (level == word.Length - 1)
                    {
                        Next[0].WordIndex = wordIndex;
                    }
                    else
                    {
                        Next[0].Put(word, level + 1, wordIndex);
                    }
                }
                else
                {
                    int ch = word[level];
                    int newStart = Math.Min(SearchTableStart, ch);
                    int newEnd = Math.Max(SearchTableEnd, ch + 1);
                    if (newStart < SearchTableStart || newEnd > SearchTableEnd)
                    {
                        var newNext = new SearchNode[newEnd - newStart];
                        Array.Copy(Next, 0, newNext, SearchTableStart - newStart, Next.Length);
                        Next = newNext;
                        SearchTableStart = newStart;
                        SearchTableEnd = newEnd;
                    }
                    //for (int i = newStart; i < newEnd; ++i)
                    //{
                    //    Next[i - newStart].DebugInfo = word.Substring(0, level) + (char)(i);
                    //}
                    if (level == word.Length - 1)
                    {
                        Next[ch - SearchTableStart].WordIndex = wordIndex;
                    }
                    else
                    {
                        Next[ch - SearchTableStart].Put(word, level + 1, wordIndex);
                    }
                }
            }
        }
    }
}
