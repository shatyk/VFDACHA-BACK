﻿using AutoMapper;
using Backend.Constants;
using Backend.Interfaces;
using Backend.Models.Requests;
using Backend.Models.Responses;
using Database;
using Microsoft.AspNetCore.Components;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Backend.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly ILanguageCacheService _languageCacheService;

        public ReportService(AppDbContext appDbContext, IMapper mapper, ILanguageCacheService languageCacheService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _languageCacheService = languageCacheService;
        }

        public async Task<IEnumerable<ReportGetAllResponse>> GetAllAsync()
        {
            int uaLanguageId = await _languageCacheService.GetLanguageIdAsync(LanguageConstants.LanguageNameUA);

            IEnumerable<Report> reports = _appDbContext.Reports.AsNoTracking()
                .Include(r => r.Contents
                    .Where(c => c.LanguageId == uaLanguageId))
                .Include(r => r.Fundraising!)
                .ThenInclude(f => f.Contents
                    .Where(c => c.LanguageId == uaLanguageId));

            return await Task.FromResult(reports.Select(_mapper.Map<ReportGetAllResponse>));
        }

        public async Task<ReportGetOneResponse> GetAsync(long id)
        {
            Report? report = _appDbContext.Reports.AsNoTracking()
                .Where(r => r.Id == id)
                .Include(r => r.Contents)
                .Include(r => r.Fundraising!)
                .Include(r => r.ReportTags)
                .FirstOrDefault();

            return await Task.FromResult(_mapper.Map<ReportGetOneResponse>(report));
        }

        public async Task<long> AddAsync(ReportAddRequest request, CancellationToken cancellationToken)
        {
            Report report = _mapper.Map<Report>(request);
            report.CreatedAt = DateTime.UtcNow;
            int languagesCount = await _languageCacheService.GetLanguagesCountAsync();
            if (report.Contents.Count() != languagesCount)
            {
                throw new HttpRequestException($"Are you DURBECEL? We have {languagesCount} languages, send me Contents for each language!", null, HttpStatusCode.BadRequest);
            }
            await _appDbContext.AddAsync(report, cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);           
            return report.Id;
        }

        public async Task UpdateAsync(ReportUpdateRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(long id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
