using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BlogApp.Helpers;
using BlogApp.Models;
using PagedList;
using PagedList.Mvc;
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Web.Configuration;

namespace BlogApp.Controllers
{
    [RequireHttps]
    public class BlogPostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
       
        // GET: BlogPosts
        public ActionResult Index(int? page, string searchString)
        {
            int pageSize = 3; // display three blog posts at a time on this page
            int pageNumber = (page ?? 1);
            var postQuery = db.Posts.OrderBy(p => p.Created).AsQueryable();
          
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                postQuery = postQuery
                    .Where(p => p.Title.Contains(searchString) ||
                                p.Body.Contains(searchString) ||
                                p.Slug.Contains(searchString) ||
                                p.Comments.Any(t => t.Body.Contains(searchString))
                           ).AsQueryable();
            }
             var postList = postQuery.ToPagedList(pageNumber, pageSize);
             ViewBag.SearchString = searchString;
             return View(postList);
        }
        
        public ActionResult Details(string slug)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts
                    .Include(p => p.Comments.Select(t => t.Author))
                    .Where(p => p.Slug == slug)
                    .FirstOrDefault();

            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }
      
        // GET: BlogPosts/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Details(string slug, string body)
        {
            if (slug == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blogPost = db.Posts
               .Where(p => p.Slug == slug)
               .FirstOrDefault();
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", blogPost);
            }
            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("Details", new { slug = slug });
        }
        
        // POST: BlogPosts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,Title,Body,MediaURL,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogPost);
                }
                if (db.Posts.Any(p => p.Slug == Slug))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogPost);
                }
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blogPost.MediaUrl = "/Uploads/" + fileName;
                }

                blogPost.Slug = Slug;
                blogPost.Created = DateTimeOffset.Now;
                db.Posts.Add(blogPost);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
                return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,Title,Body,MediaUrl,Published")] BlogPost blogPost, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var blog = db.Posts.Where(p => p.Id == blogPost.Id).FirstOrDefault();
                blog.Body = blogPost.Body;
                blog.Published = blogPost.Published;
                //blog.Slug = blogPost.Slug;
                blog.Title = blogPost.Title;
                blog.Updated = DateTime.Now;
                var Slug = StringUtilities.URLFriendly(blogPost.Title);
                if (String.IsNullOrWhiteSpace(Slug))
                {
                    ModelState.AddModelError("Title", "Invalid title");
                    return View(blogPost);
                }
                if (db.Posts.Any(p => p.Slug == Slug && p.Id != blog.Id))
                {
                    ModelState.AddModelError("Title", "The title must be unique");
                    return View(blogPost);
                }
                if (ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    blog.MediaUrl = "/Uploads/" + fileName;
                }
                blogPost.Created = DateTimeOffset.Now;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BlogPost blogPost = db.Posts.Find(id);
            if (blogPost == null)
            {
                return HttpNotFound();
            }
            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BlogPost blogPost = db.Posts.Find(id);
            db.Posts.Remove(blogPost);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateComment(string slug, string body)
        {
            if (String.IsNullOrWhiteSpace(slug))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blogPost = db.Posts
               .Where(p => p.Slug == slug)
               .FirstOrDefault();
            if (blogPost == null)
            {
                return HttpNotFound();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                TempData["ErrorMessage"] = "Comment is required";
                return RedirectToAction("Details", new { slug });
            }

            var comment = new Comment();
            comment.AuthorId = User.Identity.GetUserId();
            comment.BlogPostId = blogPost.Id;
            comment.Created = DateTime.Now;
            comment.Body = body;
            db.Comments.Add(comment);
            db.SaveChanges();
            return RedirectToAction("Details", new { slug });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
