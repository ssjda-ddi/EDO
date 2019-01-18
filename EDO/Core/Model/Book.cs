using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Util;

namespace EDO.Core.Model
{
    public class Book : IIDPropertiesProvider, ICloneable
    {
        public static void ChangeMetaDataId(List<Book> books, string oldMetaDataId, string newMetaDataId)
        {
            foreach (Book book in books)
            {
                foreach (BookRelation relation in book.BookRelations)
                {
                    if (relation.MetadataId == oldMetaDataId)
                    {
                        relation.MetadataId = newMetaDataId;
                    }
                }
            }
        }

        public static Book FindByTitle(List<Book> books, string title)
        {
            foreach (Book book in books)
            {
                if (book.Title == title)
                {
                    return book;
                }
            }
            return null;
        }

        public Book()
        {
            Id = IDUtils.NewGuid();
            BookRelations = new List<BookRelation>();
        }

        public string[] IdProperties { get { return new string[] { "Id" }; } }

        public string Id { get; set; }

        //Type
        public string BookTypeCode { get; set; }

        public bool IsBookTypeBook { get { return Options.IsBookTypeBook(BookTypeCode); } }

        public bool IsBookTypeBookChapter { get { return Options.IsBookTypeBookChapter(BookTypeCode); } }

        public bool IsBookTypeTreatiseWithPeerReview { get { return Options.IsBookTypeTreatiseWithPeerReview(BookTypeCode); } }

        public bool IsBookTypeTreatiseWithoutPeerReview { get { return Options.IsBookTypeTreatiseWithoutPeerReview(BookTypeCode); } }

        public bool IsBookTypeSocietyAbstract { get { return Options.IsBookTypeSocietyAbstract(BookTypeCode); } }

        public bool IsBookTypeReport { get { return Options.IsBookTypeReport(BookTypeCode); } }

        public bool IsBookTypeThesis { get { return Options.IsBookTypeThesis(BookTypeCode); } }

        public bool IsBookTypeWebpage { get { return Options.IsBookTypeWebpage(BookTypeCode); } }

        public bool IsBookTypeOther { get { return Options.IsBookTypeOther(BookTypeCode); } }

        //Title
        public string Title { get; set; }

        //Author
        public string Author { get; set; }

        //Editor
        public string Editor { get; set; }

        //Date of publication
        public string AnnouncementDate { get; set; }

        //Start page
        public string StartPage { get; set; }

        //End page
        public string EndPage { get; set; }

        //Publisher
        public string Publisher { get; set; }

        //City
        public string City { get; set; }

        //Book Title
        public string BookName { get; set; }

        //Journal
        public string MagazineName { get; set; }

        //Volume
        public string Volume { get; set; }

        //Issue
        public string Number { get; set; }

        //Chapter
        public string Chapter { get; set; }

        //University
        public string UniversityName { get; set; }

        //Department
        public string DepartmentName { get; set; }

        //Abstract
        public string Summary { get; set; }

        //URL
        public string Url { get; set; }

        //Language
        public string Language { get; set; }


        //Association with metadata
        public List<BookRelation> BookRelations {get; set; }

        public object Clone()
        {
            return DeepCopy(false);
        }

        public Book DeepCopy(bool keepId)
        {
            Book newBook = (Book)MemberwiseClone();
            if (keepId)
            {
                newBook.Id = Id;
            }
            newBook.BookRelations = new List<BookRelation>();
            foreach (BookRelation bookRelation in BookRelations)
            {
                newBook.BookRelations.Add(bookRelation.DeepCopy(keepId));
            }
            return newBook;
        }

        public BookRelation FindRelation(BookRelation targetRelation)
        {
            foreach (BookRelation relation in BookRelations)
            {
                if (relation.EqualsValue(targetRelation))
                {
                    return relation;
                }
            }
            return null;
        }


        public bool ContainsBookRelation(BookRelation targetRelation)
        {
            BookRelation relation = FindRelation(targetRelation);
            return relation != null;
        }

        public string GetBookId(BookRelationType type) { return IDUtils.ToId(((int)type).ToString(), Id); }

    }
}
