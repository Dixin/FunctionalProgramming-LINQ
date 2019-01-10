namespace Tutorial.GettingStarted
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Reflection;
    using System.Xml.XPath;

    internal static partial class Overview
    {
        internal static void Sql(string connectionString)
        {
            using (DbConnection connection = new SqlConnection(connectionString))
            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT [Product].[Name]
                    FROM [Production].[Product] AS [Product]
                    LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory] 
                        ON [Subcategory].[ProductSubcategoryID] = [Product].[ProductSubcategoryID]
                    LEFT OUTER JOIN [Production].[ProductCategory] AS [Category] 
                        ON [Category].[ProductCategoryID] = [Subcategory].[ProductCategoryID]
                    WHERE [Category].[Name] = @categoryName
                    ORDER BY [Product].[ListPrice] DESC";
                DbParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@categoryName";
                parameter.Value = "Bikes";
                command.Parameters.Add(parameter);
                connection.Open();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string productName = (string)reader["Name"];
                        Trace.WriteLine(productName);
                    }
                }
            }
        }
    }

    internal static partial class Overview
    {
        internal static void Xml()
        {
            XPathDocument feed = new XPathDocument("https://weblogs.asp.net/dixin/rss");
            XPathNavigator navigator = feed.CreateNavigator();
            XPathExpression selectExpression = navigator.Compile("//item[guid/@isPermaLink='true']/title/text()");
            XPathExpression sortExpression = navigator.Compile("../../pubDate/text()");
            selectExpression.AddSort(sortExpression, Comparer<DateTime>.Default);
            XPathNodeIterator nodes = navigator.Select(selectExpression);
            foreach (object node in nodes)
            {
                Trace.WriteLine(node);
            }
        }
    }

    [Pure]
    internal static partial class Overview
    {
        internal static void DelegateTypes()
        {
            Assembly coreLibrary = typeof(object).Assembly;
            IEnumerable<Type> allTypes = coreLibrary.ExportedTypes;

            // Filter delegate types from all types, and group them by namespace.
            Dictionary<string, List<Type>> delegateGroups = new Dictionary<string, List<Type>>();
            foreach (Type type in allTypes)
            {
                // Delegate type's base type is System.MulticastDelegate.
                if (type.BaseType == typeof(MulticastDelegate))
                {
                    if (!delegateGroups.TryGetValue(type.Namespace, out List<Type> delegateGroup))
                    {
                        delegateGroup = delegateGroups[type.Namespace] = new List<Type>();
                    }
                    delegateGroup.Add(type);
                }
            }

            // Sort delegate type groups by count (descending), and then by namespace (ascending).
            List<KeyValuePair<string, List<Type>>> sortedDelegateGroups = new List<KeyValuePair<string, List<Type>>>();
            foreach (KeyValuePair<string, List<Type>> nextGroup in delegateGroups)
            {
                for (int index = 0; index <= sortedDelegateGroups.Count; index++)
                {
                    if (index < sortedDelegateGroups.Count)
                    {
                        KeyValuePair<string, List<Type>> currentGroup = sortedDelegateGroups[index];
                        int compare = currentGroup.Value.Count - nextGroup.Value.Count;
                        if (compare == 0)
                        {
                            compare = string.CompareOrdinal(nextGroup.Key, currentGroup.Key);
                        }

                        if (compare >= 0)
                        {
                            continue;
                        }
                    }

                    sortedDelegateGroups.Insert(index, nextGroup);
                    break;
                }
            }

            // Output the results.
            foreach (KeyValuePair<string, List<Type>> delegateGroup in sortedDelegateGroups)
            {
                Trace.Write(delegateGroup.Value.Count + " in " + delegateGroup.Key + ":");
                foreach (Type delegateType in delegateGroup.Value)
                {
                    Trace.Write(" " + delegateType.Name);
                }
                Trace.Write(Environment.NewLine);
            }
            // 27 in System: Action`1 Action Action`2 Action`3 Action`4 Func`1 Func`2 Func`3 Func`4 Func`5 Action`5 Action`6 Action`7 Action`8 Func`6 Func`7 Func`8 Func`9 Comparison`1 Converter`2 Predicate`1 AssemblyLoadEventHandler AsyncCallback EventHandler EventHandler`1 ResolveEventHandler UnhandledExceptionEventHandler
            // 8 in System.Threading: WaitCallback WaitOrTimerCallback IOCompletionCallback TimerCallback ContextCallback ParameterizedThreadStart SendOrPostCallback ThreadStart
            // 3 in System.Reflection: MemberFilter ModuleResolveEventHandler TypeFilter
            // 3 in System.Runtime.CompilerServices: TryCode CleanupCode CreateValueCallback
        }
    }

    internal class Crawler
    {
        private readonly DirectoryInfo downloadDirectory;

        internal Crawler(DirectoryInfo downloadDirectory)
        {
            this.downloadDirectory = downloadDirectory;
        }

        // Download the specified URI to the download directory.
        internal FileInfo Download(Uri sourceUri)
        {
            throw new NotImplementedException();
        }
    }

    internal class Template
    {
        private readonly FileInfo templateFile;

        internal Template(FileInfo templateFile)
        {
            this.templateFile = templateFile;
        }

        // Convert the specified HTML document with template.
        internal FileInfo Convert(FileInfo sourceFile)
        {
            throw new NotImplementedException();
        }
    }

    internal class DocumentBuilder
    {
        private readonly Crawler crawler;

        private readonly Template template;

        internal DocumentBuilder(Crawler crawler, Template template)
        {
            this.crawler = crawler;
            this.template = template;
        }

        internal FileInfo Build(Uri uri)
        {
            FileInfo htmlDocument = this.crawler.Download(uri);
            return this.template.Convert(htmlDocument);
        }
    }

    internal partial class Overview
    {
        internal static void BuildDocumentWithObjects(Uri sourceUri, DirectoryInfo downloadDirectory, FileInfo templateFile)
        {
            DocumentBuilder builder = new DocumentBuilder(new Crawler(downloadDirectory), new Template(templateFile));
            FileInfo resultFile = builder.Build(sourceUri);
        }
    }
}
