using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading.Tasks;

namespace AppDomainTestApp
{
    class Program
    {
        static AppDomain _unDefaultDomain = AppDomain.CreateDomain("UnDefaultAppDomain", null, null);
        static void Main(string[] args)
        {
            /**
             * 注
             * “默认AppDomain”
             * 1、创建该对象的AppDomain，称作该对象的“默认AppDomain”
             * 
             * “非默认AppDomain”
             * 1、跨AppDomain访问该对象的AppDomain，称作该对象的“非默认AppDomain”
             * 
             * “代理对象”
             * 1、跨AppDomain访问的对象并不是对象本体，是被访问的对象的代理对象。
             * 2、该代理对象对外与源对象相同，只有自身知道自身是代理对象，且在自身内部维护跨AppDomain的生命周期
             * 
             * (ILease)obj.GetLifetimeService();
             * 1、obj必须继承MarshalByRefObject。否则无法获取生命周期   证实
             * 2、obj必须实现ISponsor。否则无法在非默认AppDomain获取生命周期    待证实
             * 3、obj必须实现ISponsor，并且必须被非默认AppDomain对象绑定才能在默认AppDomain获取生命周期  证实
             * 
             * ILease.Register(ISponsor);
             * 1、ILease必须遵循上方3条规则
             * 2、ILease称为“绑定者”，ISponsor称为“被绑定者”，
             *      绑定者可以跨AppDomain访问ISponsor，为自身的代理对象在ISponsor的AppDomain中增加生命周期
             *      
             *      
             *  =====================================================================================================
             *  跨AppDomain通信已被标记为“过时”
             *  不必纠结其实现细节，只当做双方对对方的生命周期计时，并且使用某个双工协议进行通信
             *  例如：用使用一个线程轮询生命周期是否到期，使用TCP传递通信数据
             */

            //默认AppDomain创建的对象
            var defaultDomainSponsor = new DefaultAppDomainTaskBySponsor();
            var defaultDomainTask = new DefaultAppDomainTask();
            var testc = new Test();

            //非默认AppDomain
            var unDefaultDomainType = typeof(UnDefaultAppDomain);
            var unDefaultDomain = (UnDefaultAppDomain)_unDefaultDomain.CreateInstanceAndUnwrap(unDefaultDomainType.Assembly.FullName, unDefaultDomainType.FullName);
            //非默认AppDomain创建的对象
            var unDefaultDomainTask = unDefaultDomain.CreateDomainTask();

            //默认AppDomain注册到非默认AppDomain
            var unLease = (ILease)unDefaultDomainTask.GetLifetimeService();
            unLease.Register(defaultDomainSponsor);

            //非默认AppDomain注册到默认AppDomain
            var lease = (ILease)defaultDomainSponsor.GetLifetimeService();
            lease.Register(unDefaultDomainTask);
            RegisterSponsor(defaultDomainTask, unDefaultDomainTask);
            RegisterSponsor(testc, unDefaultDomainTask);


            Console.Read();
        }

        // 需要让非默认AppDomain中的CrawlTask对象不断给默认AppDomain中的对象“续租时间”
        static void RegisterSponsor(object obj, ISponsor sponsor)
        {
            ILease lease = (ILease)((MarshalByRefObject)obj).GetLifetimeService();
            lease.Register(sponsor);
        }
    }


    class DefaultAppDomain : MarshalByRefObject
    {

    }

    class DefaultAppDomainTaskBySponsor : MarshalByRefObject, ISponsor
    {
        public DateTime CreateTime { get; private set; }
        public int RenewalCount { get; private set; }
        public DefaultAppDomainTaskBySponsor()
        {
            CreateTime = DateTime.Now;
        }

        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            lease.InitialLeaseTime = TimeSpan.FromSeconds(5);
            return lease;
        }

        public TimeSpan Renewal(ILease lease)
        {
            RenewalCount++;
            return TimeSpan.FromSeconds(5);
        }
    }

    class Test : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            lease.InitialLeaseTime = TimeSpan.FromSeconds(4);
            return lease;
        }
    }

    class DefaultAppDomainTask : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            lease.InitialLeaseTime = TimeSpan.FromSeconds(4);
            return lease;
        }
    }

    class UnDefaultAppDomain : MarshalByRefObject
    {
        public UnDefaultAppDomainTaskBySponsor CreateDomainTask()
        {
            return new UnDefaultAppDomainTaskBySponsor();
        }
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            lease.InitialLeaseTime = TimeSpan.FromSeconds(5);
            return lease;
        }
    }

    class UnDefaultAppDomainTaskBySponsor : MarshalByRefObject, ISponsor
    {
        public DateTime CreateTime { get; private set; }
        public int RenewalCount { get; private set; }
        public UnDefaultAppDomainTaskBySponsor()
        {
            CreateTime = DateTime.Now;
        }
        public override object InitializeLifetimeService()
        {
            ILease lease = (ILease)base.InitializeLifetimeService();
            lease.InitialLeaseTime = TimeSpan.FromSeconds(5);
            return lease;
        }
        public TimeSpan Renewal(ILease lease)
        {
            RenewalCount++;
            return TimeSpan.FromSeconds(5);
        }
    }
}
