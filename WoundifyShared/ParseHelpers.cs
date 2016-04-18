using System;
using System.Collections.Generic;
using System.Text;

namespace WoundifyShared
{
    class ConstituencyTreeNode
    {
        public System.Collections.Generic.List<ConstituencyTreeNode> Children = new System.Collections.Generic.List<ConstituencyTreeNode>();

        public string PoS { get; set; }
        public string word { get; set; }
        public ConstituencyTreeNode parentNode { get; set; }
        public ConstituencyTreeNode nextNode { get; set; }
        public ConstituencyTreeNode previousNode { get; set; }
        public ConstituencyTreeNode nextWord { get; set; }
        public ConstituencyTreeNode previousWord { get; set; }
        public int level;
        public int maxDepth;

        public ConstituencyTreeNode(string _PoS)
        {
            PoS = _PoS;
        }

        public ConstituencyTreeNode(string _PoS, string _word)
        {
            PoS = _PoS;
            word = _word;
        }

        public ConstituencyTreeNode AddChild(ConstituencyTreeNode node)
        {
            Children.Add(node);
            return node;
        }
    }

    class ParseHelpers
    {
        public static string TextFromConstituencyTree(ConstituencyTreeNode node)
        {
            return TextFromConstituencyTree(node, "").Trim();
        }

        private static string TextFromConstituencyTree(ConstituencyTreeNode node, string tree)
        {
            ConstituencyTreeNode previous = null;
            foreach (ConstituencyTreeNode n in node.Children)
            {
                if (previous != n.previousNode)
                    throw new FormatException();
                if (previous != null && previous.nextNode != n)
                    throw new FormatException();
                previous = n;
                if (n.word == null)
                {
                    tree += " (" + n.PoS;
                    tree = TextFromConstituencyTree(n, tree);
                    tree += ")";
                }
                else
                    tree += " (" + n.PoS + " " + n.word + ")";
            }
            return tree;
        }

        public static string WordsFromConstituencyTree(ConstituencyTreeNode node)
        {
            return WordsFromConstituencyTree(node, "");
        }

        private static string WordsFromConstituencyTree(ConstituencyTreeNode node, string words)
        {
            node = node.nextWord;
            if (node != null)
            {
                words += node.word + " ";
                words = WordsFromConstituencyTree(node, words);
            }
            return words;
        }

        public static string[] FormatConstituencyTree(ConstituencyTreeNode node)
        {
            string[] printLines = new string[node.maxDepth];
            FormatConstituencyTree(node, 0, 0, printLines, 0);
            return printLines;
        }

        private static void FormatConstituencyTree(ConstituencyTreeNode node, int level, int col, string[] printLines, int debugLevel)
        {
            if (level != node.level)
                throw new FormatException();
            if (printLines[level] == null)
                printLines[level] = "|";
            if (col - printLines[level].Length - 1 > 0)
                printLines[level] += string.Empty.PadRight(col - printLines[level].Length - 1, ' ') + "|";
            foreach (ConstituencyTreeNode n in node.Children)
            {
                string debugInfo = debugLevel < 4 ? string.Empty : "(" + n.parentNode.PoS + " " + n.level + "/" + n.maxDepth + ")";
                if (n.word == null)
                {
                    int previousLen = printLines[level].Length;
                    FormatConstituencyTree(n, level + 1, previousLen, printLines, debugLevel);
                    int len = printLines[level + 1].Length - previousLen - n.PoS.Length - 1;
                    int left = len / 2;
                    int right = len - left;
                    printLines[level] += string.Empty.PadRight(left, '-') + n.PoS + debugInfo + string.Empty.PadRight(right, '-') + "|";
                }
                else
                    printLines[level] += " " + n.PoS + debugInfo + ":" + n.word + " |";
            }
        }

        public static ConstituencyTreeNode ConstituencyTreeFromText(string parse)
        {
            parse = parse.Replace(" (", "(");
            ConstituencyTreeNode root = new ConstituencyTreeNode("root");
            parse = ConstituencyTreeFromText(parse, root, root);
            return root;
        }

        private static string ConstituencyTreeFromText(string parse, ConstituencyTreeNode node, ConstituencyTreeNode root)
        {
            ConstituencyTreeNode previous = null;
            while (!string.IsNullOrWhiteSpace(parse) && parse[0] == '(')
            {
                int lastIndex = parse.IndexOfAny(" ()".ToCharArray(), 1);
                char lastChar = parse[lastIndex];
                string gu = parse.Substring(1, lastIndex - 1);
                parse = parse.Substring(lastIndex).TrimStart();
                ConstituencyTreeNode child;
                switch (lastChar)
                {
                    case ' ':
                        lastIndex = parse.IndexOfAny("()".ToCharArray());
                        string wo = parse.Substring(0, lastIndex);
                        parse = parse.Substring(lastIndex).TrimStart();
                        child = node.AddChild(new ConstituencyTreeNode(gu, wo));
                        child.level = node.level + 1;
                        if (root.nextWord == null)
                            root.nextWord = child;
                        else
                            root.previousWord.nextWord = child;
                        child.previousWord = root.previousWord;
                        root.previousWord = child;
                        break;
                    case '(':
                        child = node.AddChild(new ConstituencyTreeNode(gu));
                        child.level = node.level + 1;
                        parse = ConstituencyTreeFromText(parse, child, root);
                        break;
                    default:
                        throw new FormatException();
                }
                if (parse[0] != ')')
                    throw new FormatException();
                parse = parse.Substring(1).TrimStart();
                child.parentNode = node;
                child.previousNode = previous;
                if (previous != null)
                    previous.nextNode = child;
                previous = child;
                if (child.maxDepth + 1 > node.maxDepth)
                    node.maxDepth = child.maxDepth + 1;
            }
            return parse;
        }
    }
}
