using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossblog.Controllers;
using crossblog.Domain;
using crossblog.Model;
using crossblog.Repositories;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace crossblog.tests.Controllers {
    public class CommentsControllerTests {
        private CommentsController _commentsController;

        private Mock<ICommentRepository> _commentsRepositoryMock = new Mock<ICommentRepository> ();
        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository> ();

        public CommentsControllerTests () {
            _commentsController = new CommentsController (_articleRepositoryMock.Object, _commentsRepositoryMock.Object);
        }

        [Fact]
        public async Task Get_NotFound () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            // Act
            var result = await _commentsController.Get (1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Get_ReturnsEmptyList () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            var commentsDbSetMock = Builder<Comment>.CreateListOfSize (1).Build ().ToAsyncDbSetMock ();
            _commentsRepositoryMock.Setup (m => m.Query ()).Returns (commentsDbSetMock.Object);
            // Act
            var result = await _commentsController.Get (3);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull (content);

            Assert.Equal (0, content.Comments.Count ());
        }

        [Fact]
        public async Task Get_ReturnsList () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            var commentsDbSetMock = Builder<Comment>.CreateListOfSize (3).Build ().ToAsyncDbSetMock ();
            _commentsRepositoryMock.Setup (m => m.Query ()).Returns (commentsDbSetMock.Object);

            // Act
            var result = await _commentsController.Get (1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as CommentListModel;
            Assert.NotNull (content);

            Assert.Equal (1, content.Comments.Count ());
        }

        [Fact]
        public async Task Get_ByCommentId_ArticleIdNotFound () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            // Act
            var result = await _commentsController.Get (1, 1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Get_ByCommentId_CommentIdNotFound () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            var commentsDbSetMock = Builder<Comment>.CreateListOfSize (1).Build ().ToAsyncDbSetMock ();
            _commentsRepositoryMock.Setup (m => m.Query ()).Returns (commentsDbSetMock.Object);

            // Act
            var result = await _commentsController.Get (1, -1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Get_ByCommentId_ReturnsItem () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            var commentsDbSetMock = Builder<Comment>.CreateListOfSize (3).Build ().ToAsyncDbSetMock ();
            _commentsRepositoryMock.Setup (m => m.Query ()).Returns (commentsDbSetMock.Object);

            // Act
            var result = await _commentsController.Get (1, 1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull (content);

            Assert.Equal ("Title1", content.Title);
        }

        [Fact]
        public async Task Post_BadRequest () {
            //Arrange 
            _commentsController.ModelState.Clear ();
            _commentsController.ModelState.AddModelError ("Title", "Title is Required");

            //Act
            var result = await _commentsController.Post (1, It.IsAny<CommentModel> ());

            //Assert
            Assert.NotNull (result);

            var objectResult = result as BadRequestObjectResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Post_ArticleNotFound () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            //Act
            var result = await _commentsController.Post (1, Builder<CommentModel>.CreateNew ().Build ());

            // Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Post_Item () {
            //Arrange 
            _commentsRepositoryMock.Setup (m => m.InsertAsync (It.IsAny<Comment> ())).
            Callback<Comment> (a => a.Id = 1)
                .Returns (Task.CompletedTask);

            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            //Act
            var result = await _commentsController.Post (1, Builder<CommentModel>.CreateNew ().Build ());

            //Assert
            Assert.NotNull (result);
            var objectResult = result as CreatedResult;
            Assert.NotNull (objectResult);

            Assert.Equal ("articles/1/comments/1", objectResult.Location);

            var content = objectResult.Value as CommentModel;
            Assert.NotNull (content);
            Assert.Equal ("Title1", content.Title);
        }
    }
}