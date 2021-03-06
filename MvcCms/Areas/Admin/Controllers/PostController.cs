﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvcCms.Data;
using MvcCms.Models;

namespace MvcCms.Areas.Admin.Controllers
{
    // /admin/post
    [RouteArea("Admin")]
    [RoutePrefix("post")]
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostRepository _repository;
        private readonly IUserRepository _users;

        public PostController() : 
            this(new PostRepository(), new UserRepository())
        {            
        }

        public PostController(IPostRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _users = userRepository;
        }

        // GET: Admin/Post
        [Route("")]
        public async Task<ActionResult> Index(string searchString)
        {
            IEnumerable<Post> posts;
            if(!User.IsInRole("author"))
            {
                posts = await GetAllPosts(searchString);                
            }
            else
            {
                var user = await GetLoggedInUser();
                posts = await GetAllPosts(searchString, user.Id);                
            }            

            return View(posts);
        }

        // /admin/post/create
        [HttpGet]
        [Route("create")]
        public ActionResult Create()
        {            
            return View(new Post());
        }

        // /admin/post/create
        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Post model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetLoggedInUser();

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                model.Id = model.Title;
            }

            model.Id = model.Id.MakeUrlFriendly();
            model.Tags = model.Tags.Select(t => t.MakeUrlFriendly()).ToList();
            model.Created = DateTime.Now;
            model.AuthorId = user.Id;

            try
            {
                _repository.Create(model);

                return RedirectToAction("index");
            }
            catch (Exception e)
            {
                ModelState.AddModelError("key", e);
                return View(model);
            }
        }

        // /admin/post/edit/post-to-edit
        [HttpGet]
        [Route("edit/{postId}")]
        public async Task<ActionResult> Edit(string postId)
        {                      
            var post = await _repository.GetAsync(postId);

            if(post == null)
            {
                return HttpNotFound();
            }

            if(User.IsInRole("author"))
            {
                var user = await GetLoggedInUser();
                if(post.AuthorId != user.Id)
                {
                    return new HttpUnauthorizedResult();
                }
            }

            return View(post);
        }

        // /admin/post/edit/post-to-edit
        [HttpPost]
        [Route("edit/{postId}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string postId, Post model)
        {            
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            if (User.IsInRole("author"))
            {
                var user = await GetLoggedInUser();
                var post = await _repository.GetAsync(postId);
                if (post != null && post.AuthorId != user.Id)
                {
                    return new HttpUnauthorizedResult();
                }
            }

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                model.Id = model.Title;
            }

            model.Id = model.Id.MakeUrlFriendly();
            model.Tags = model.Tags.Select(t => t.MakeUrlFriendly()).ToList();

            try
            {
                _repository.Edit(postId, model);

                return RedirectToAction("index");
            }
            catch(KeyNotFoundException)
            {
                return HttpNotFound();
            }
            catch(Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View(model);
            }
        }

        [HttpGet]
        [Route("delete/{postId}")]
        [Authorize(Roles = "admin, editor")]
        public async Task<ActionResult> Delete(string postId)
        {            
            var post = await _repository.GetAsync(postId);

            if (post == null)
            {
                return HttpNotFound();
            }

            return View(post);
        }

        // /admin/post/delete/post-to-edit
        [HttpPost]
        [Route("delete/{postId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin, editor")]
        public ActionResult Delete(string postId, string dummy)
        {            
            try
            {
                _repository.Delete(postId);

                return RedirectToAction("index");
            }
            catch (KeyNotFoundException)
            {
                return HttpNotFound();
            }            
        }

        private async Task<CmsUser> GetLoggedInUser()
        {
            return await _users.GetUserByNameAsync(User.Identity.Name);
        }

        private async Task<IEnumerable<Post>> GetAllPosts(string searchString, string userId = null)
        {
            IEnumerable<Post> posts;
            if(string.IsNullOrWhiteSpace(searchString))
            {
                if(userId == null)
                {
                    posts = await _repository.GetAllAsync();
                }
                else
                {
                    posts = await _repository.GetPostsByAuthorAsync(userId);
                }
            }
            else
            {
                Expression<Func<Post, bool>> predicate = p => p.Title.Contains(searchString);
                if(userId == null)
                {
                    posts = await _repository.GetAllAsync(predicate);
                }
                else
                {
                    posts = await _repository.GetPostsByAuthorAsync(userId, predicate);
                }                
            }

            return posts;
        }

        private bool _isDisposed;

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                _users.Dispose(); 
                _repository.Dispose();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }
    }
}