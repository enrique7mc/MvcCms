﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcCms.Models;

namespace MvcCms.Data
{
    public class PostRepository : IPostRepository
    {
        public Post Get(string id)
        {
            using (var db = new CmsContext())
            {
                return db.Posts.Include("Author").SingleOrDefault(post => post.Id == id);
            }
        }

        public void Edit(string id, Post updatedItem)
        {
            using(var db = new CmsContext())
            {
                var post = db.Posts.SingleOrDefault(p => p.Id == id);

                if(post == null)
                {
                    throw new KeyNotFoundException("A post with the id of " + id + " does not exists.");
                }

                post.Id = updatedItem.Id;
                post.Title = updatedItem.Title;
                post.Content = updatedItem.Content;
                post.Published = updatedItem.Published;
                post.Tags = updatedItem.Tags;

                db.SaveChanges();
            }
        }

        public void Create(Post model)
        {
            using(var db = new CmsContext())
            {
                var post = db.Posts.SingleOrDefault(p => p.Id == model.Id);

                if(post != null)
                {
                    throw new ArgumentException("A post with id " + model.Id + " already exists.");
                }

                if(string.IsNullOrWhiteSpace(model.Id))
                {
                    model.Id = model.Title;
                }

                model.Id = model.Id.MakeUrlFriendly();
                model.Tags = model.Tags.Select(t => t.MakeUrlFriendly()).ToList();
                db.Posts.Attach(model);
                db.SaveChanges();
            }
        }

        public IEnumerable<Post> GetAll()
        {
            using (var db = new CmsContext())
            {
                return db.Posts.Include("Author").OrderByDescending(p => p.Created).ToArray();
            }
        }
    }
}
