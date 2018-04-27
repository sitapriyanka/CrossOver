using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using crossblog.tests.Controllers;

namespace crossblog
{
    public class Program
    {
        public static void Main(string[] args)
        {
          CommentsControllerTests test = new CommentsControllerTests();
          test.Post_Item();
        }
         
    }
}
