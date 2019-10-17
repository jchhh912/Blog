﻿using MeowvBlog.Core;
using MeowvBlog.Core.Dto;
using MeowvBlog.Core.Dto.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MeowvBlog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class BlogController : ControllerBase
    {
        private readonly MeowvBlogDBContext _context;

        public BlogController(MeowvBlogDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据URL获取文章详情
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("post")]
        public async Task<Response<GetPostDto>> GetPostAsync(string url)
        {
            var response = new Response<GetPostDto>();

            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Url.Equals(url));
            if (null == post)
            {
                response.Msg = $"URL：{url} 不存在";
                return response;
            }
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == post.CategoryId);
            var tags = (from post_tags in await _context.PostTags.ToListAsync()
                        join tag in await _context.Tags.ToListAsync()
                        on post_tags.TagId equals tag.Id
                        where post_tags.PostId.Equals(post.Id)
                        select new TagDto
                        {
                            TagName = tag.TagName,
                            DisplayName = tag.DisplayName
                        }).ToList();
            var previous = await _context.Posts.Where(x => x.CreationTime > post.CreationTime).Take(1).FirstOrDefaultAsync();
            var next = await _context.Posts.Where(x => x.CreationTime < post.CreationTime).OrderByDescending(x => x.CreationTime).Take(1).FirstOrDefaultAsync();

            var result = new GetPostDto
            {
                Title = post.Title,
                Author = post.Author,
                Url = post.Url,
                Html = post.Html,
                Markdown = post.Markdown,
                CreationTime = Convert.ToDateTime(post.CreationTime).ToString("MMMM dd, yyyy HH:mm:ss", new CultureInfo("en-us")),
                Category = new CategoryDto { CategoryName = category.CategoryName, DisplayName = category.DisplayName },
                Tags = tags,
                Previous = new PostForPagedDto { Title = previous.Title, Url = previous.Url },
                Next = new PostForPagedDto { Title = next.Title, Url = next.Url }
            };

            response.Result = result;
            return response;
        }
    }
}