﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VelocityDb;
using VelocityDb.Session;

namespace Verify
{
  class Verify
  {
    static int Main(string[] args)
    {
      if (args.Length == 0)
      {
        System.Console.WriteLine("ERROR, no boot path specified. Restart Verify and add bootup database path as a command line parameter");
        return 1;
      }
      int ct = 0;
      try
      {
        using (SessionNoServer session = new SessionNoServer(args[0]))
        {
          DataCache.MaximumMemoryUse = 5000000000; // 5 GB, set this to what fits your case
          session.BeginRead();
          List<Database> dbList = session.OpenAllDatabases();
          foreach (Database db in dbList)
            foreach (Page page in db)
              foreach (IOptimizedPersistable iPers in page)
              {
                object obj = iPers.WrappedObject;
                ++ct;
                if (iPers.WrappedObject is IOptimizedPersistable)
                {
                  UInt64 id = iPers.Id;
                  OptimizedPersistable pObj = iPers as OptimizedPersistable;
                  if (pObj != null)
                  {
                    pObj.LoadFields();
                    foreach (OptimizedPersistable fObj in pObj.OptimizedPersistableFieldValues())
                    {
                      fObj.LoadFields();
                    }
                    foreach (object value in pObj.GetFieldValues())
                    {
                      WeakIOptimizedPersistableReferenceBase weakRef = value as WeakIOptimizedPersistableReferenceBase;
                      if (weakRef != null)
                        if (session.Open(weakRef.Id) == null)
                          throw new UnexpectedException("WeakRefence object is null");
                    }
                  }
                }
                else if (obj is string)
                  continue;
                else if (obj is Array)
                  continue;
                IEnumerable anEnum = obj as IEnumerable;
                if (anEnum != null)
                  foreach (object o in anEnum)
                  {
                  }
              }
          session.Commit();
        }
        Console.WriteLine("OK, verfied " + ct + " objects");
        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine("FAILED, verfied " + ct + " objects");
        Console.WriteLine(ex.ToString());
        return -1;
      }
    }
  }
}
