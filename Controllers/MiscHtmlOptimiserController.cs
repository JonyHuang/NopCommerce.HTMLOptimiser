﻿using Nop.Core;
using Nop.Plugin.Misc.HtmlOptimiser.Code;
using Nop.Plugin.Misc.HtmlOptimiser.Models;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Nop.Plugin.Misc.HtmlOptimiser.Controllers
{
    [AdminAuthorize]
    public class MiscHtmlOptimiserController : BaseController
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;

        public MiscHtmlOptimiserController(ISettingService settingService,
                                            IStoreService storeService,
                                            IWorkContext workContext,
                                            IWebHelper webHelper)
        {
            _settingService = settingService;
            _storeService = storeService;
            _workContext = workContext;
            _webHelper = webHelper;
        }

        private void RestartPlugin()
        {
            // re-executes the startup task to reinitialize the HTML filter
            var startUpTask = new StartupTask();

            startUpTask.Execute();
        }

        #region Configure

        [HttpGet]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            model.MinificationEnabled = settings.MinificationEnabled;

            return View("Nop.Plugin.Misc.HtmlOptimiser.Views.MiscHtmlOptimiser.Configure", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid) 
            {
                return Configure();
            }

            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            settings.MinificationEnabled = model.MinificationEnabled;

            _settingService.SaveSetting(settings, x => x.MinificationEnabled, storeScope, false);
            
            _settingService.ClearCache();

            RestartPlugin();

            return Configure();
        }

        #endregion

        #region Save Settings

        private void SaveSettings(HtmlOptimiserSettings settings)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            _settingService.SaveSetting(settings, x => x.RemoveHeaders, storeScope, false);
            _settingService.SaveSetting(settings, x => x.AddHeaders, storeScope, false);

            _settingService.ClearCache();

            WebConfigUpdater.UpdateWebConfig(settings.RemoveHeaders);

            RestartPlugin();
        }

        #endregion

        #region Remove Headers

        [HttpPost]
        public ActionResult RemoveHeaders(DataSourceRequest command)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            var removeHeaders = (settings.RemoveHeaders ?? Enumerable.Empty<string>())
                                    .Select((h, i) => new RemoveHeaderModel { Index = (i + 1), Name = h })
                                    .ToList();

            var model = new DataSourceResult()
            {
                Data = removeHeaders,
                Total = removeHeaders.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost]
        public ActionResult InsertRemoveHeaders(RemoveHeaderModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            if (settings.RemoveHeaders == null)
            {
                settings.RemoveHeaders = new List<string>();
            }

            settings.RemoveHeaders.Add(model.Name);

            SaveSettings(settings);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult UpdateRemoveHeaders(RemoveHeaderModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            settings.RemoveHeaders[model.Index - 1] = model.Name;

            _settingService.SaveSetting(settings, x => x.RemoveHeaders, storeScope, false);
            
            _settingService.ClearCache();

            SaveSettings(settings);

            RestartPlugin();

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult DeleteRemoveHeaders(int Index)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            settings.RemoveHeaders.RemoveAt(Index - 1);

            _settingService.SaveSetting(settings, x => x.RemoveHeaders, storeScope, false);
            
            _settingService.ClearCache();

            SaveSettings(settings);

            RestartPlugin();

            return new NullJsonResult();
        }

        #endregion

        #region Add Headers

        [HttpPost]
        public ActionResult AddHeaders(DataSourceRequest command)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            var addHeaders = (settings.AddHeaders ?? new List<AddHeader>())
                                            .Select((h, i) =>
                                            {
                                                return new AddHeaderModel { Index = (i + 1), Name = h.Name, Value = h.Value };
                                            })
                                            .ToList();

            var model = new DataSourceResult()
            {
                Data = addHeaders,
                Total = addHeaders.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost]
        public ActionResult InsertAddHeaders(AddHeaderModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            if (settings.AddHeaders == null)
            {
                settings.AddHeaders = new List<AddHeader>();
            }

            settings.AddHeaders.Add(new AddHeader { Name = model.Name, Value = model.Value });

            SaveSettings(settings);

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult UpdateAddHeaders(AddHeaderModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            settings.AddHeaders[model.Index - 1].Name = model.Name;
            settings.AddHeaders[model.Index - 1].Value = model.Value;

            _settingService.SaveSetting(settings, x => x.AddHeaders, storeScope, false);

            _settingService.ClearCache();

            SaveSettings(settings);

            RestartPlugin();

            return new NullJsonResult();
        }

        [HttpPost]
        public ActionResult DeleteAddHeaders(int Index)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

            var settings = _settingService.LoadSetting<HtmlOptimiserSettings>(storeScope);

            settings.AddHeaders.RemoveAt(Index - 1);

            _settingService.SaveSetting(settings, x => x.AddHeaders, storeScope, false);

            _settingService.ClearCache();

            SaveSettings(settings);

            RestartPlugin();

            return new NullJsonResult();
        }

        #endregion
    }
}
