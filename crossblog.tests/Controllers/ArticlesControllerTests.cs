﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class ArticlesControllerTests {
        private ArticlesController _articlesController;

        private Mock<IArticleRepository> _articleRepositoryMock = new Mock<IArticleRepository> ();

        public ArticlesControllerTests () {
            _articlesController = new ArticlesController (_articleRepositoryMock.Object);
        }

        [Fact]
        public async Task Search_ReturnsEmptyList () {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize (3).Build ().ToAsyncDbSetMock ();
            _articleRepositoryMock.Setup (m => m.Query ()).Returns (articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search ("Invalid");

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull (content);

            Assert.Equal (0, content.Articles.Count ());
        }

        [Fact]
        public async Task Search_ReturnsList () {
            // Arrange
            var articleDbSetMock = Builder<Article>.CreateListOfSize (3).Build ().ToAsyncDbSetMock ();
            _articleRepositoryMock.Setup (m => m.Query ()).Returns (articleDbSetMock.Object);

            // Act
            var result = await _articlesController.Search ("Title");

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as ArticleListModel;
            Assert.NotNull (content);

            Assert.Equal (3, content.Articles.Count ());
        }

        [Fact]
        public async Task Get_NotFound () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (1)).Returns (Task.FromResult<Article> (null));

            // Act
            var result = await _articlesController.Get (1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Get_ReturnsItem () {
            // Arrange
            _articleRepositoryMock.Setup (m => m.GetAsync (1)).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            // Act
            var result = await _articlesController.Get (1);

            // Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as ArticleModel;
            Assert.NotNull (content);

            Assert.Equal ("Title1", content.Title);
        }

        //modify
        [Fact]
        public async Task Post_BadRequest () {
            //Arrange 
            _articlesController.ModelState.Clear ();
            _articlesController.ModelState.AddModelError ("Title", "Title is Required");

            //Act
            var result = await _articlesController.Post (It.IsAny<ArticleModel> ());

            //Assert
            Assert.NotNull (result);
            var objectResult = result as BadRequestObjectResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Post_Item () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.InsertAsync (It.IsAny<Article> ())).
            Callback<Article> (a => a.Id = 1)
                .Returns (Task.CompletedTask);

            //Act
            var result = await _articlesController.Post (Builder<ArticleModel>.CreateNew ().Build ());

            //Assert
            Assert.NotNull (result);
            var objectResult = result as CreatedResult;
            Assert.NotNull (objectResult);

            Assert.Equal ("articles/1", objectResult.Location);

            var content = objectResult.Value as Article;
            Assert.NotNull (content);
            Assert.Equal ("Title1", content.Title);
        }

        [Fact]
        public async Task Put_BadRequest () {
            //Arrange 
            _articlesController.ModelState.Clear ();
            _articlesController.ModelState.AddModelError ("Title", "Title is Required");

            //Act
            var result = await _articlesController.Put (It.IsAny<int> (), It.IsAny<ArticleModel> ());

            //Assert
            Assert.NotNull (result);

            var objectResult = result as BadRequestObjectResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Put_NotExistingItem () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            //Act
            var result = await _articlesController.Put (It.IsAny<int> (), It.IsAny<ArticleModel> ());

            //Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Put_ExistingItem () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));
            _articleRepositoryMock.Setup (m => m.UpdateAsync (It.IsAny<Article> ())).Returns (Task.CompletedTask);

            //Act
            var result = await _articlesController.Put (It.IsAny<int> (), new ArticleModel () { Id = 1, Title = "TestTitleUpdate" });

            //Assert
            Assert.NotNull (result);

            var objectResult = result as OkObjectResult;
            Assert.NotNull (objectResult);

            var content = objectResult.Value as Article;
            Assert.NotNull (content);

            Assert.Equal ("TestTitleUpdate", content.Title);
        }

        [Fact]
        public async Task Delete_NotExistingItem () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (null));

            //Act
            var result = await _articlesController.Delete (It.IsAny<int> ());

            //Assert
            Assert.NotNull (result);

            var objectResult = result as NotFoundResult;
            Assert.NotNull (objectResult);
        }

        [Fact]
        public async Task Delete_ExistingItem () {
            //Arrange 
            _articleRepositoryMock.Setup (m => m.DeleteAsync (It.IsAny<Article> ())).Returns (Task.CompletedTask);
            _articleRepositoryMock.Setup (m => m.GetAsync (It.IsAny<int> ())).Returns (Task.FromResult<Article> (Builder<Article>.CreateNew ().Build ()));

            //Act
            var result = await _articlesController.Delete (It.IsAny<int> ());

            //Assert
            Assert.NotNull (result);

            var objectResult = result as OkResult;
            Assert.NotNull (objectResult);
        }

    }
}