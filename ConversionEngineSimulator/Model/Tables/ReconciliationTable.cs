﻿using System.Collections.Generic;

namespace ConversionEngineSimulator
{
    public class ReconciliationTable : IDatabaseTable
    {
        public ReconciliationTable()
        {
            ColumnNames = "ID, AFSTEMTDATO, DESCRIPTION, MANAGER, CONTEXT, " +
                          "SRCANTAL, DSTANTAL, CUSTOMANTAL, AFSTEMRESULTAT, RUN_JOB, " +
                          "TOOLKIT_ID, SRC_SQL_COST, DST_SQL_COST, CUSTOM_SQL_COST, SRC_SQL, " +
                          "DST_SQL, CUSTOM_SQL, SRC_SQL_TIME, DST_SQL_TIME, CUSTOM_SQL_TIME, " +
                          "START_TIME, END_TIME, AFSTEMNINGSDATA";

            OutputColumnNames = "@ID, @AFSTEMTDATO, @DESCRIPTION, @MANAGER, @CONTEXT, " +
                                "@SRCANTAL, @DSTANTAL, @CUSTOMANTAL, @AFSTEMRESULTAT, @RUN_JOB, " +
                                "@TOOLKIT_ID, @SRC_SQL_COST, @DST_SQL_COST, @CUSTOM_SQL_COST, @SRC_SQL, " +
                                "@DST_SQL, @CUSTOM_SQL, @SRC_SQL_TIME, @DST_SQL_TIME, @CUSTOM_SQL_TIME, " +
                                "@START_TIME, @END_TIME, @AFSTEMNINGSDATA";
            TableName = "dbo.AFSTEMNING";
            Entries = DbUtilities.QueryTable<Reconciliation>(this);
            Entries.Sort();
        }
        public string ColumnNames { get; }

        public string OutputColumnNames { get; }

        public string TableName { get; }
        public List<Reconciliation> Entries { get; set; }
    }
}