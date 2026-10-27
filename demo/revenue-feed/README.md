# Revenue feed demo files

Stand-in billing files from GMR's revenue system. Upload one to a run on the **Revenue feed** page; processing starts in the background.

| File | What it shows |
|---|---|
| `01-clean.csv` | Green run: 4 lines applied to 3 accounts / opportunities (one account-level line). |
| `02-line-errors.csv` | Amber run: 1 line applied; rejected lines for unknown account, opportunity of another account, bad amount, bad date, duplicate reference. |
| `03-corrupt.csv` | Red run: binary content, fails at *Validate file*, nothing applied. |
| `04-wrong-columns.csv` | Red run: semicolon-delimited with unknown columns, fails at *Validate file*. |
| `01-clean.csv` again | Grey run: identical file already processed, *Skipped*. |
| `05-overlap.csv` | Green run: `INV-2026-10-0001` skipped as already applied, the new line applied. |

Recovery demo: after `02-line-errors.csv`, set account code `AMCN-9999` on an account (or fix the data) and click **Reprocess** on the run: only the rejected lines run again, nothing is applied twice.

Expected header: `BillingReference,AccountCode,OpportunityCode,PeriodStart,PeriodEnd,Amount` (UTF-8, comma-separated, dates `yyyy-MM-dd`, dot decimal separator). `OpportunityCode` may be empty for account-level billing; accounts match on **Code**, opportunities on **Revenue system code**.
