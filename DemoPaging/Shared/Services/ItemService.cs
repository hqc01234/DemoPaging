using DemoPaging.DBContext;
using DemoPaging.Entities;
using DemoPaging.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace DemoPaging.Services
{
    public class ItemService
    {
        public IObservable<PagingOffsetData> ItemsOffsetPaging => _itemsOffsetPaging;

        public IObservable<PagingOffsetData> ItemsOffsetPagingOptimized => _itemsOffsetPagingOpt;

        public IObservable<PagingCursorData> ItemsCursorPaging => _itemsCursorPaging;

        public IObservable<bool> ItemsOffsetPagingLoading => _itemsOffsetPagingLoading;

        public IObservable<bool> ItemsOffsetPagingOptLoading => _itemsOffsetPagingOptLoading;

        public IObservable<bool> ItemsCursorPagingLoading => _itemsCursorPagingLoading;

        public IObservable<string> ExecTime => _execTime;

        private readonly BehaviorSubject<PagingOffsetData> _itemsOffsetPaging;
        private readonly BehaviorSubject<PagingOffsetData> _itemsOffsetPagingOpt;
        private readonly BehaviorSubject<PagingCursorData> _itemsCursorPaging;
        private readonly Subject<bool> _itemsOffsetPagingLoading;
        private readonly Subject<bool> _itemsOffsetPagingOptLoading;
        private readonly Subject<bool> _itemsCursorPagingLoading;
        private readonly Subject<string> _execTime;
        private readonly DemoDbContext _demoDbContext;

        public ItemService(DemoDbContext demoDbContext)
        {
            _demoDbContext = demoDbContext;
            _itemsOffsetPaging = new BehaviorSubject<PagingOffsetData>(new PagingOffsetData());
            _itemsOffsetPagingOpt = new BehaviorSubject<PagingOffsetData>(new PagingOffsetData());
            _itemsCursorPaging = new BehaviorSubject<PagingCursorData>(new PagingCursorData());
            _itemsOffsetPagingLoading = new Subject<bool>();
            _itemsOffsetPagingOptLoading = new Subject<bool>();
            _itemsCursorPagingLoading = new Subject<bool>();
            _execTime = new Subject<string>();
        }

        public async Task GetItemsOffsetPaging(PagingOffsetArgs args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var query = _demoDbContext.Items;

            _itemsOffsetPagingLoading.OnNext(true);
            _itemsOffsetPaging.OnNext(new PagingOffsetData
            {
                Items = await query.OrderBy(x => x.CreateDate)
                    .Skip(args.PageIndex * args.PageSize)
                    .Take(args.PageSize).ToListAsync(),
                Total = await query.CountAsync()
            });

            watch.Stop();

            _execTime.OnNext($"Execution Time: {watch.ElapsedMilliseconds} ms");
            _itemsOffsetPagingLoading.OnNext(false);
        }

        public async Task GetItemsOffsetPagingOptimized(PagingOffsetArgs args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var query = _demoDbContext.Items;

            Expression<Func<Item, bool>> whereExp = (Item item) => query
                .OrderBy(x => x.CreateDate)
                .Select(y => y.Id)
                .Skip(args.PageIndex * args.PageSize)
                .Take(args.PageSize).Contains(item.Id);

            _itemsOffsetPagingOptLoading.OnNext(true);
            _itemsOffsetPagingOpt.OnNext(new PagingOffsetData
            {
                Items = await query.Where(whereExp).ToListAsync(),
                Total = await query.CountAsync()
            });

            watch.Stop();

            _execTime.OnNext($"Execution Time: {watch.ElapsedMilliseconds} ms");
            _itemsOffsetPagingOptLoading.OnNext(false);
        }

        public async Task GetItemsCursorPaging(PagingCursorArgs args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var query = _demoDbContext.Items;

            var time = args.PageCursor != null ? new DateTimeOffset(args.PageCursor.Value, TimeSpan.Zero) : new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);

            _itemsCursorPagingLoading.OnNext(true);

            // get page size + 1 item (for next cursor data)
            var items = await query
                .OrderBy(x => x.CreateDate)
                .Where(x => x.CreateDate >= time)
                .Take(args.PageSize + 1).ToListAsync();

            // get previous cursor
            var firstItem = items.FirstOrDefault()?.CreateDate;
            var previous = await query
                .OrderByDescending(x => x.CreateDate)
                .Where(x => x.CreateDate < firstItem)
                .Take(args.PageSize).LastOrDefaultAsync();

            _itemsCursorPaging.OnNext(new PagingCursorData
            {
                Items = items.Take(args.PageSize).ToList(),
                Total = await query.CountAsync(),
                NextPage = items.LastOrDefault()?.CreateDate.Ticks,
                PreviousPage = previous?.CreateDate.Ticks
            });

            watch.Stop();

            _execTime.OnNext($"Execution Time: {watch.ElapsedMilliseconds} ms");
            _itemsCursorPagingLoading.OnNext(false);
        }

        public async Task Delete(PagingCursorArgs args)
        {
            if (args.PageCursor == null)
            {
                return;
            }

            var query = _demoDbContext.Items;
            var time = args.PageCursor != null ? new DateTimeOffset(args.PageCursor.Value, TimeSpan.Zero) : new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);

            _itemsCursorPagingLoading.OnNext(true);

            var previous = await query
                .OrderByDescending(x => x.CreateDate)
                .Where(x => x.CreateDate < time)
                .Take(args.PageSize).ToListAsync();

            if (previous.Count == 0)
            {
                return;
            }

            int index = new Random().Next(previous.Count);
            var itemDelete = previous[index];

            _demoDbContext.Items.Remove(itemDelete);
            await _demoDbContext.SaveChangesAsync();

            _itemsCursorPagingLoading.OnNext(false);

            return;
        }
    }
}
