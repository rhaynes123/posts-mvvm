# Rider Phantom Build Errors — Causes & Fixes

Research date: 2026-11-04
Symptom: red squiggles / errors in the Rider editor, but `Build > Build Solution` (and `dotnet build`) succeeds.

## Likely causes, ranked by frequency

### 1. Stale ReSharper / analysis engine cache
Rider's editor analysis runs on a separate cache from the actual compiler. When the cache drifts out of sync with the code (after a branch switch, package upgrade, or a crashed indexer), the editor keeps showing errors the compiler no longer sees.

**Fix:** `File | Invalidate Caches…`, keep **ReSharper Caches** checked (always on by default), then click **Invalidate and Restart**. If that is not enough, re-run it and also tick **Clear file system cache and Local History**.

**Why it works:** Invalidating deletes the .NET analysis engine cache and the VFS cache; they rebuild from disk on next open.

**Source:** [JetBrains — Invalidate caches](https://www.jetbrains.com/help/rider/Invalidate_caches.html)

### 2. Stale `bin/` and `obj/` from a prior build or branch
Phantom "ambiguous reference" / "type already defined" / missing-symbol errors frequently come from leftover intermediate assemblies or an out-of-date `obj/project.assets.json` (NuGet restore graph).

**Fix:** Close Rider. Delete every `bin/` and `obj/` folder in the solution. Reopen Rider, let it restore NuGet packages, then **Build | Rebuild Solution**.

**Why it works:** Rider's analyzer reads the restore graph and prior compile outputs; wiping them forces a clean restore + compile.

**Source:** community consensus, no primary source — but referenced in multiple JetBrains support threads (e.g. [red squiggly lines all over my files](https://rider-support.jetbrains.com/hc/en-us/community/posts/360010156979-red-squiggly-lines-all-over-my-files), [Errors in solution that do not really exist](https://rider-support.jetbrains.com/hc/en-us/community/posts/360009392060-Errors-in-solution-that-do-not-really-exist)).

### 3. NuGet packages not restored / restore desync
If `obj/project.assets.json` is missing or stale, the editor cannot resolve types that the compiler can (the CLI restores on build; the editor does not always).

**Fix:** Right-click the solution → **Restore NuGet Packages**. Or from a terminal: `dotnet restore`. Then reload the solution (**File | Reload All from Disk**).

**Source:** [JetBrains — Package Restore](https://www.jetbrains.com/help/rider/Using_NuGet.html) (NuGet integration docs).

### 4. Roslyn analyzers failing to load (source generators, .NET SDK mismatch)
Source generators and third-party Roslyn analyzers run in-process. If they fail to load, their generated types vanish from the editor model → red squiggles everywhere, even though the CLI build (which loads them correctly) succeeds. This has repeatedly been a confirmed bug on new .NET SDK releases.

**Fix (in order):**
1. Check **Settings | Build, Execution, Deployment | Toolset and Build** — confirm Rider is pointed at the right MSBuild / .NET SDK.
2. If a `global.json` pins an SDK, confirm it is installed (`dotnet --list-sdks`).
3. Toggle **Settings | Editor | Inspection Settings | Roslyn Analyzers → Enable Roslyn analyzers** off/on, then restart.
4. Update Rider — false-error waves from Roslyn worker bugs have been fixed in point releases (e.g. RIDER-98962, RIDER-140334).

**Sources:**
- [JetBrains — Roslyn Analyzers settings](https://www.jetbrains.com/help/rider/Settings_Roslyn_Analyzers.html)
- [JetBrains — .NET Compiler Platform (Roslyn) analyzers](https://www.jetbrains.com/help/rider/Using_NET_Compiler_Analyzers.html)
- [YouTrack RIDER-98962 — .NET 8.0 RC1 breaks Source Generators](https://youtrack.jetbrains.com/issue/RIDER-98962)
- [YouTrack RIDER-140334 — Rider Roslyn Worker broken on .NET 10](https://youtrack.jetbrains.com/projects/RIDER/issues/RIDER-140334)

### 5. Target framework / SDK mismatch
If `global.json` pins an SDK that isn't installed, or the `<TargetFramework>` references a TFM the installed SDK does not ship, Rider resolves references against the wrong framework and shows types as missing.

**Fix:** `dotnet --list-sdks` → confirm the SDK in `global.json` (if any) is installed. Confirm every TFM in the `.csproj` matches an installed targeting pack. Update either the SDK or the `global.json`.

**Source:** [Microsoft — global.json overview](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json).

### 6. Solution-wide analysis paused, or indexing not finished
If SWEA is paused or Rider hasn't finished indexing after open, cross-file symbols appear "unresolved" until analysis completes.

**Fix:** Bottom-right status bar — check the circle icon for solution-wide analysis status; click to resume. Wait for the indexing progress bar to finish before trusting any squiggle.

**Source:** [JetBrains — Inspection Settings (Solution-Wide Analysis)](https://www.jetbrains.com/help/rider/Settings_Inspection_Settings.html).

### 7. Known Rider bug on your version
Some phantom-error waves are just IDE bugs. The UE 5.7 / Rider 2025 "`Cannot resolve symbol 'remove_reference_t'`" wave was fixed in **Rider 2026.1 EAP 5**.

**Fix:** Update Rider via Toolbox. Search [YouTrack RIDER](https://youtrack.jetbrains.com/issues/RIDER) for your exact error string before spending time on local workarounds.

**Source:** [RIDER-134494 — False positive errors after UE 5.5.3 → 5.7.1](https://youtrack.jetbrains.com/projects/RIDER/issues/RIDER-134494).

## Escalation path

Try in this order. Stop at the first one that works:

1. **Wait** — let indexing + SWEA finish. Check the status bar.
2. **File | Reload All from Disk** — cheap, fixes drift after external file edits.
3. **Build | Rebuild Solution** — regenerates `obj/*.assets.json` and source-generator output.
4. **Restore NuGet Packages** on the solution (or `dotnet restore` in a terminal).
5. **File | Invalidate Caches… → Invalidate and Restart** (ReSharper Caches checked).
6. **Same as 5, but also check "Clear file system cache and Local History"**.
7. **Nuclear option:**
   - Close Rider.
   - Delete all `bin/` and `obj/` folders in the solution.
   - Delete the `.idea/` folder at the solution root.
   - Reopen Rider → let it re-index and restore.
   - **Build | Rebuild Solution**.
8. **Still broken?** Update Rider. Then search [YouTrack](https://youtrack.jetbrains.com/issues/RIDER) for the exact error message — if it's a known bug, file/vote on the issue rather than fighting the IDE.
