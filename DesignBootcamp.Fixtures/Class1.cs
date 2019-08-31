using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesignBootcamp.Fixtures
{
    public class DocumentServiceFixture
    {
        [Fact]
        public void Cache_hits_should_return_value_from_cache_test()
        {
            Document doc = new Document { Id = "1"};
            var cacheMock = new Mock<IDocumentCache>();
            cacheMock.Setup(c => c.Get("1")).Returns(doc);
            var service = new DocumentService(new SqlDocumentDb().WithCaching(cacheMock.Object));
            var document = service.GetDocument("1");
            document.Should().NotBeNull();
            cacheMock.Verify(c => c.Get("1"), Times.Once);
        }


        [Fact]
        public void Cache_hits_should_not_call_actual_db_test()
        {
            Document doc = new Document { Id = "1" };
            var dbMock = new Mock<IDocumentDb>();
            var cacheMock = new Mock<IDocumentCache>();
            cacheMock.Setup(c => c.Get("1")).Returns(doc);
            var service = new DocumentService(dbMock.Object.WithCaching(cacheMock.Object));
            var document = service.GetDocument("1");
            document.Should().NotBeNull();
            cacheMock.Verify(c => c.Get("1"), Times.Once);
            dbMock.Verify(db => db.GetById("1"), Times.Never);
        }


        [Fact]
        public void Cache_misses_should_call_actual_db_test()
        {
            Document doc = new Document { Id = "1" };
            var dbMock = new Mock<IDocumentDb>();
            var cacheMock = new Mock<IDocumentCache>();
            cacheMock.Setup(c => c.Get("1")).Returns<Document>(null);
            dbMock.Setup(db => db.GetById("1")).Returns(doc);
            var service = new DocumentService(dbMock.Object.WithCaching(cacheMock.Object));

            var document = service.GetDocument("1");
            document.Should().NotBeNull();
            cacheMock.Verify(c => c.Get("1"), Times.Once);
            dbMock.Verify(db => db.GetById("1"), Times.Once);
        }

        [Fact]
        public void Cache_misses_should_populate_cache_with_actual_db_value_test()
        {
            Document doc = new Document { Id = "1" };
            var dbMock = new Mock<IDocumentDb>();
            var cacheMock = new Mock<IDocumentCache>();
            cacheMock.Setup(c => c.Get("1")).Returns<Document>(null);
            dbMock.Setup(db => db.GetById("1")).Returns(doc);
            var service = new DocumentService(dbMock.Object.WithCaching(cacheMock.Object));

            var document = service.GetDocument("1");
            document.Should().NotBeNull();
            cacheMock.Verify(c => c.Get("1"), Times.Once);
            dbMock.Verify(db => db.GetById("1"), Times.Once);
            cacheMock.Verify(c => c.Set("1", doc), Times.Once);
        }

    }
}
