﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Goals_Site.Data;
using Goals_Site.Models;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Http;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using OpenXMLTemplates.Variables;
using OpenXMLTemplates.Engine;
using Newtonsoft.Json.Linq;
using Grpc.Core;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Goals_Site.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Jobs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Job.Include(j => j.Client).Include(j => j.Project_manager).Include(j => j.Sales_manager);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Jobs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Job == null)
            {
                return NotFound();
            }

            var job = await _context.Job
                .Include(j => j.Client)
                .Include(j => j.Project_manager)
                .Include(j => j.Sales_manager)
                .FirstOrDefaultAsync(m => m.JobId == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: Jobs/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Client, "ClientId", "Name");
            ViewData["Project_managerId"] = new SelectList(_context.Project_manager, "Project_managerId", "Name");
            ViewData["Sales_managerId"] = new SelectList(_context.Sales_manager, "Sales_managerId", "Name");
            ViewData["Site_Id"] = new SelectList(_context.Site, "SiteId", "Address");
            return View();
        }

        // POST: Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("JobId,Job_number,Date,Start,Est_finish_time,Scope,Project_managerId,Sales_managerId,ClientId,From_site,To_site")] Job job)
        {
            //if (ModelState.IsValid)
            //{

            // CONTEXT IS READ ONLY THIS WONT WORK
            //var pManag = await _context.Project_manager.FindAsync(job.Project_managerId);
            //job.Project_manager = pManag;

            //var sManag = await _context.Sales_manager.FindAsync(job.Sales_managerId);
            //job.Sales_manager = sManag;

            //var client = await _context.Client.FindAsync(job.ClientId);
            //job.Client = client;

            int temp = Int32.Parse(job.From_site);
            int temp2 = Int32.Parse(job.To_site);
            Site fSite = await _context.Site.FindAsync(temp);
            job.From_site = fSite.Address;
            Site tSite = await _context.Site.FindAsync(temp2);
            job.To_site = tSite.Address;

            job.From_siteID = temp;
            job.To_siteID = temp2;

            _context.Add(job);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            //}
            
            //ViewData["ClientId"] = new SelectList(_context.Client, "ClientId", "ClientId", job.ClientId);
            //ViewData["Project_managerId"] = new SelectList(_context.Project_manager, "Project_managerId", "Project_managerId", job.Project_managerId);
            //ViewData["Sales_managerId"] = new SelectList(_context.Sales_manager, "Sales_managerId", "Sales_managerId", job.Sales_managerId);
            //return View(job);
        }

        // GET: Jobs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Job == null)
            {
                return NotFound();
            }

            var job = await _context.Job.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Client, "ClientId", "ClientId", job.ClientId);
            ViewData["Project_managerId"] = new SelectList(_context.Project_manager, "Project_managerId", "Project_managerId", job.Project_managerId);
            ViewData["Sales_managerId"] = new SelectList(_context.Sales_manager, "Sales_managerId", "Sales_managerId", job.Sales_managerId);
            return View(job);
        }

        // POST: Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("JobId,Job_number,Date,Start,Est_finish_time,Scope,Project_managerId,Sales_managerId,ClientId,From_site,To_site")] Job job)
        {
            if (id != job.JobId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.JobId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Client, "ClientId", "ClientId", job.ClientId);
            ViewData["Project_managerId"] = new SelectList(_context.Project_manager, "Project_managerId", "Project_managerId", job.Project_managerId);
            ViewData["Sales_managerId"] = new SelectList(_context.Sales_manager, "Sales_managerId", "Sales_managerId", job.Sales_managerId);
            return View(job);
        }

        // GET: Jobs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Job == null)
            {
                return NotFound();
            }

            var job = await _context.Job
                .Include(j => j.Client)
                .Include(j => j.Project_manager)
                .Include(j => j.Sales_manager)
                .FirstOrDefaultAsync(m => m.JobId == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Job == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Job'  is null.");
            }
            var job = await _context.Job.FindAsync(id);
            if (job != null)
            {
                _context.Job.Remove(job);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST Create Crate Document
        [HttpPost, ActionName("CrateSheet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrateSheet(int id)
        { 
            using var doc = new OpenXMLTemplates.Documents.TemplateDocument("Templates/CrateTemp.docx");

            if (_context.Job == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Job'  is null.");
            }

            var job = await _context.Job.FindAsync(id);


            System.Collections.IDictionary valueDictionary = new Dictionary<string, string>
            {
                { "JobNumber",  job.Job_number.ToString() },
                { "ProjectManager", _context.Project_manager.Find(job.Project_managerId).Name.ToString() },
                { "DeliveryDate", job.Date.ToString() }, // Do not confuse with Scheduled Move Date -> Although from Job.Date Field
                { "DeliveryTime", job.Start.ToString() },// Do not confuse with Scheduled Move Date -> Although from Job.Time Field
                { "ClientName", _context.Client.Find(job.ClientId).Name.ToString() },
                { "ClientContact", _context.Client.Find(job.ClientId).Contact_Name.ToString() },
                { "ClientTelephone", _context.Client.Find(job.ClientId).Contact_Phone.ToString() },
                { "ClientEmail", _context.Client.Find(job.ClientId).Email.ToString() },
                { "ClientReference", _context.Client.Find(job.ClientId).Reference.ToString() },
                { "DeliveryAddress", job.To_site.ToString() },
                { "SiteContact", _context.Site.Find(job.To_siteID).Contact_Name.ToString() },
                { "Telephone", _context.Site.Find(job.To_siteID).Contact_Phone.ToString() }

                
                // Features to still be added
                // Issue, Delivery Address, Site Contact, Telephone, Delivery Instructions, Accounts Info, Collection Address, Collection Date

            };

            var src = new VariableSource(valueDictionary);
            var engine = new DefaultOpenXmlTemplateEngine();
            engine.ReplaceAll(doc, src);
            doc.SaveAs("Documents/CrateSheets/ " + job.Job_number.ToString() + "_" + job.Client.Name.ToString() + "_CrateSheet.docx");
            return RedirectToAction(nameof(Index));
            

        }


        // POST Create Job Document
        [HttpPost, ActionName("JobSheet")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JobSheet(int id)
        {
            using var doc = new OpenXMLTemplates.Documents.TemplateDocument("Templates/Simplified.docx");

            //if (job != null)
            //{
            //return RedirectToAction(nameof(Index));
            //}

            Job job = _context.Job.Find(id);
            System.Collections.IDictionary valueDictionary = new Dictionary<string, string>
            {
                { "JobNumber",  job.Job_number.ToString() },
                { "ProjectManager", "Antonio" }
            };

            var src = new VariableSource(valueDictionary);
            var engine = new DefaultOpenXmlTemplateEngine();
            engine.ReplaceAll(doc, src);
            doc.SaveAs("Documents/JobSheets/" + job.Job_number.ToString() + "_" + job.Client.Name.ToString() + "_JobSheet.docx");
            return RedirectToAction(nameof(Index));


        }


        private bool JobExists(int id)
        {
          return _context.Job.Any(e => e.JobId == id);
        }
    }
}
