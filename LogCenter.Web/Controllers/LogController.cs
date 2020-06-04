﻿using LogCenter.Business;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using SSO.Util.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace LogCenter.Web.Controllers
{
    public class LogController : Controller
    {
        protected MsQueue<LogModel> msQueue = new MsQueue<LogModel>(AppSettings.GetValue("msqueue"));
        protected Log log = new Log();
        public ActionResult Insert(LogModel logModel)
        {
            msQueue.SendMessage(logModel, "log");
            return new ResponseModel<string>(ErrorCode.success, "");
        }
        [OutputCache(Duration = 60 * 60 * 4)]
        public ActionResult GetFromList()
        {
            var result = log.GetFromList().ToJson();
            return new ResponseModel<string>(ErrorCode.success, result);
        }
        [HttpPost]
        public ActionResult GetList(LogListModel logModel)
        {
            long count = 0;
            var filter = log.GetLogFilter(logModel.From, logModel.ControllerName, logModel.ActionName, logModel.StartTime, logModel.EndTime, logModel.UserId, logModel.UserName);
            var result = log.GetPageList(filter, null, null, logModel.Sorts, logModel.PageIndex, logModel.PageSize, ref count).ToJson().ReplaceJsonString();
            return new ResponseModel<string>(ErrorCode.success, result, count);
        }
        [HttpPost]
        public ActionResult GetListSimple(LogListModel logModel)
        {
            long count = 0;
            var filter = log.GetLogFilter(logModel.From, logModel.ControllerName, logModel.ActionName, logModel.StartTime, logModel.EndTime, logModel.UserId, logModel.UserName);
            var includeFields = new List<string>() { "Controller", "Action", "CreateTime", "UserName" };
            var result = log.GetPageList(filter, null, includeFields, logModel.Sorts, logModel.PageIndex, logModel.PageSize, ref count).ToJson().ReplaceJsonString();
            return new ResponseModel<string>(ErrorCode.success, result, count);
        }
        public ActionResult GetById(string id)
        {
            var result = log.FindOne(ObjectId.Parse(id)).ToJson().ReplaceJsonString();
            return new ResponseModel<string>(ErrorCode.success, result);
        }
        /// <summary>
        /// 最近几天操作记录
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public ActionResult RecordByDay(int last = 30)
        {
            var result = log.OpRecordDay(DateTime.Now.AddDays(-last)).ToJson();
            return new ResponseModel<string>(ErrorCode.success, result);
        }
    }


}