﻿using PersonalSite.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PersonalSite.DataAccess
{
    public class BlogAccessor : AccessorBase
    {
        public string GetBlogContent(string name)
        {
            var blogDirectory = FilePath("~/blogs");

            var potentialEntry = blogDirectory + "\\" + name + ".html";

            if (!System.IO.File.Exists(potentialEntry))
                return null;

            var content = System.IO.File.ReadAllText(potentialEntry);

            return content;
        }

        public List<BlogViewModel> GetContentForBlogNames(IEnumerable<BlogMetadataViewModel> entries, int amount)
        {
            var previews = new List<BlogViewModel>();

            foreach (var entry in entries)
            {
                amount--;

                var content = GetBlogContent(entry.Name);

                if (content != null)
                    previews.Add(new BlogViewModel()
                    {
                        Content = content,
                        Name = entry.Name,
                        Published = entry.Published
                    });

                if (amount < 1)
                    break;
            }

            return previews;
        }

        public XElement GetRssBase()
        {
            var directory = FilePath("~/blogs/metadata");

            var path = directory + "\\rss-base.xml";

            var rssBase = XElement.Load(path);

            return rssBase;
        }

        public BlogMetadataViewModel[] GetOrderedBlogMetadata(bool allowPreview = false, bool withDescription = false)
        {
            var metadataDirectory = FilePath("~/blogs/metadata");

            var metadataPath = metadataDirectory + "\\blog-metadata.xml";

            var blogMetaData = XElement.Load(metadataPath);

            var entries =
                blogMetaData.Elements("Entry").Select(n =>
                    new BlogMetadataViewModel()
                    {
                        Name = n.Attribute("Name").Value,
                        Published = DateTime.Parse(n.Attribute("Published").Value),
                        Description = withDescription ? Regex.Replace(n.Element("Description").Value, @"\s+", " ") : null
                    })
                    .Where(i => i.Published <= DateTime.Now || allowPreview)
                    .ToArray();

            return entries;
        }

        public List<YearArchiveViewModel> GetArchives()
        {
            var entries = GetOrderedBlogMetadata();

            var archives = new List<YearArchiveViewModel>();
            var mfi = new DateTimeFormatInfo();

            foreach (var entry in entries)
            {
                var yearItem = archives.SingleOrDefault(i => i.Year == entry.Published.Year);

                if (yearItem == null)
                {
                    yearItem = new YearArchiveViewModel()
                    {
                        Year = entry.Published.Year,
                        Text = entry.Published.Year.ToString(),
                        Months = new List<MonthArchiveViewModel>()
                    };

                    archives.Add(yearItem);
                }

                var monthItem = yearItem.Months.SingleOrDefault(i => i.Month == entry.Published.Month);

                if (monthItem == null)
                {
                    monthItem = new MonthArchiveViewModel()
                    {
                        Month = entry.Published.Month,
                        Text = mfi.GetMonthName(entry.Published.Month),
                        Articles = new List<BlogMetadataViewModel>()
                    };

                    yearItem.Months.Add(monthItem);
                }

                monthItem.Articles.Add(entry);
            }

            return archives;
        }
    }
}
