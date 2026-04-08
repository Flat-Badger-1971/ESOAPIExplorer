# Improvement Tasks

Build status during scan: `Build successful`.

This document captures implementation tasks found while scanning the solution for startup risk, loading issues, MVVM violations, and general maintainability problems.

## Priority 1: Startup and navigation hardening

### 1. Await application initialization and surface startup failures
**Files:** `ESOAPIExplorer/App.xaml.cs`, `ESOAPIExplorer.Services/NavigationService.cs`
- [x] Make `OnLaunched` startup flow explicitly asynchronous-safe.
- [x] Await `navigation.InitializeAsync()` instead of fire-and-forget startup.
- [x] Add error handling around startup/navigation initialization and show a user-visible failure state.
- [x] Replace `Container = RegisterDependencyInjection;` property-backed construction with a single explicit service-provider build method.

**Why:** Exceptions during startup can currently be lost, causing partial app initialization and blank UI states.

### 2. Fix navigation service implementation defects
**Files:** `ESOAPIExplorer.Services/NavigationService.cs`
- [x] Remove duplicate `MainFrame.Navigate(pageType)` call in `InternalNavigateToAsync`.
- [x] Remove or replace `Window.Current` usage; it is not a safe WinUI 3 pattern.
- [x] Replace reflection-based window lookup with injected app/window abstractions.
- [x] Validate page resolution and fail with a clear message if a page mapping is missing.
- [x] Implement or remove `PopToRootAsync()`.
- [x] Implement `CurrentApplication_NavigationFailed` with actionable diagnostics instead of `NotImplementedException`.
- [x] Review whether `GoToAsync` should always clear the back stack.

**Why:** The current implementation can silently fail, contains dead code paths, and has runtime `NotImplementedException` paths.

## Priority 2: Remove UI-thread blocking and race conditions

### 3. Replace synchronous waits and ad-hoc background work in `HomeViewModel`
**Files:** `ESOAPIExplorer.ViewModels/HomeViewModel.cs`
- [x] Remove `FilterItemsAsync().Wait()` from `FilterText` setter.
- [x] Replace `Thread.Sleep(300)` debounce logic with an async debounce/cancellation pattern.
- [x] Avoid `Task.Run` for view-model state transitions unless it is truly CPU-bound work.
- [x] Dispose and centralize `CancellationTokenSource` usage.
- [x] Ensure null-safe behavior when filtering before `AllItems` is populated.

**Why:** `.Wait()` on the UI path can freeze the app, and the current task/cancellation flow is race-prone.

### 4. Reduce expensive nested parallelism in selection and search helpers
**Files:** `ESOAPIExplorer.ViewModels/HomeViewModel.cs`, `ESOAPIExplorer.Services/ESODocumentationService.cs`, `ESOAPIExplorer.Models/Search/Fzy.cs`
- [x] Review nested `Parallel.ForEach` usage and replace with simpler sequential code where data size does not justify parallel overhead.
- [x] Replace repeated `Contains` checks on concurrent collections with `HashSet`/dictionary lookups.
- [x] Measure whether parallel search actually improves responsiveness before keeping it.

**Why:** Over-parallelization increases complexity, can harm responsiveness, and makes loading behavior harder to reason about.

## Priority 3: Enforce MVVM boundaries

### 5. Move picker/window dependencies out of view models
**Files:** `ESOAPIExplorer.ViewModels/SettingsViewModel.cs`, `ESOAPIExplorer.ViewModels/ExportViewModel.cs`, `ESOAPIExplorer.Services/ESODocumentationService.cs`
- [x] Remove direct `Window`, `FileOpenPicker`, `FileSavePicker`, and `FolderPicker` creation from view models.
- [x] Introduce an abstraction such as `IFilePickerService` / `IFolderPickerService`.
- [x] Inject picker services into view models instead of using `Application.Current` reflection.
- [x] Keep HWND/window initialization inside infrastructure services only.

**Why:** This breaks MVVM boundaries and couples view models to platform UI types, making loading and testing harder.

### 6. Remove UI types from view-model state
**Files:** `ESOAPIExplorer.ViewModels/SettingsViewModel.cs`
- [x] Replace `ComboBoxItem` as backing state with plain strings or enums.
- [x] Bind controls to simple values instead of view objects.

**Why:** View models should expose data, not UI controls.

## Priority 4: Make documentation loading more robust

### 7. Harden documentation initialization and cache recovery
**Files:** `ESOAPIExplorer.Services/ESODocumentationService.cs`
- [x] Stop swallowing all exceptions with `Console.WriteLine` only.
- [x] Return a clear failure result when cache load or parsing fails.
- [x] Add recovery behavior for corrupt cache files.
- [x] Guard all call sites against `Documentation == null`.
- [x] Replace recursive `GetPathAsync()` retry logic with an iterative flow.
- [x] Avoid constructing picker state in the service constructor if it can be deferred.

**Why:** A failed parse/cache load can leave the app running with invalid state and no visible error.

### 8. Review first-load performance and user feedback
**Files:** `ESOAPIExplorer.Services/ESODocumentationService.cs`, `ESOAPIExplorer.ViewModels/HomeViewModel.cs`
- [x] Add an explicit busy/loading state during initial documentation load.
- [x] Report progress or at least a "loading documentation" message.
- [x] Cache only after successful parse validation.

**Why:** First load appears heavy and currently offers little feedback if it stalls.

## Priority 5: Fix correctness and safety issues

### 9. Restore equality checks in `ViewModelBase.SetProperty`
**Files:** `ESOAPIExplorer.ViewModels/Base/ViewModelBase.cs`
- [x] Re-enable equality comparison before raising `PropertyChanged`.
- [x] Review affected setters for any code that relied on duplicate notifications.

**Why:** Every assignment currently raises `PropertyChanged`, which can trigger unnecessary UI refreshes and command cascades.

### 10. Fix null-safety issues in converters and helpers
**Files:** `ESOAPIExplorer.ValueConverters/ConstantToNumberConverter.cs`, other converters
- [x] Guard against `ConstantValues.GetConstantValue` returning `null`.
- [x] Return safe fallback values instead of assuming every lookup succeeds.
- [x] Review all `ConvertBack` methods that throw `NotImplementedException` and confirm they are never used two-way.

**Why:** Missing data can currently cause runtime null reference failures during binding.

### 11. Fix dialog event-subscription leak
**Files:** `ESOAPIExplorer.Services/DialogService.cs`
- [x] Unsubscribe `ResponseEntered` handlers after dialog completion.
- [x] Ensure repeated dialog usage does not accumulate callbacks on the shared view model.

**Why:** The current implementation can retain handlers and invoke stale callbacks.

### 12. Improve command robustness
**Files:** `ESOAPIExplorer.ViewModels/Base/RelayCommandGeneric.cs`
- [x] Make generic command execution tolerant of `null`/type mismatch where appropriate.
- [x] Add optional typed `canExecute` support for parameterized commands.

**Why:** Direct casting in commands can throw at runtime if a binding passes an unexpected parameter.

## Priority 6: Clean up architecture and duplication

### 13. Consolidate duplicated utility naming and ownership
**Files:** `ESOAPIExplorer.Models/Utility.cs`, `ESOAPIExplorer.ViewModels/Utility.cs`, `ESOAPIExplorer.Services/Utility.cs`
- [x] Review the three `Utility` classes and split them into purpose-specific types.
- [x] Rename them to reflect responsibility, e.g. `PrimeNumberUtility`, `SearchAlgorithmDiscovery`, `LuaTypeInference`.

**Why:** Current naming increases ambiguity and makes maintenance harder.

### 14. Centralize page/view registration strategy
**Files:** `ESOAPIExplorer/App.xaml.cs`, `ESOAPIExplorer.Services/NavigationService.cs`
- [x] Decide whether pages are created through DI or direct frame navigation.
- [x] Register all navigable views consistently if DI-backed page creation is intended.
- [x] Remove unused page factory methods if they are not part of the chosen design.

**Why:** The code currently mixes DI resolution and `Frame.Navigate(Type)` patterns.

## Priority 7: Product backlog items already present in code

### 15. Convert inline TODOs into tracked work
**Files:** `ESOAPIExplorer.ViewModels/HomeViewModel.cs`, `ESOAPIExplorer.ViewModels/SettingsViewModel.cs`, `ESOAPIExplorer.ViewModels/ExportViewModel.cs`
- [x] Settings binding cleanup.
- [x] Enum list delay diagnosis.
- [x] Busy indicator implementation.
- [x] Export workflow completion.
- [x] Constant-value search.
- [x] Theme/padding polish.
- [x] Rescan implementation.

Tracked backlog items created from inline TODOs:
- [x] Investigate formatter/alignment cleanup in `HomeView.xaml` and related detail templates.
- [x] Review settings persistence/binding flow between `SettingsViewModel`, local settings, and dependent view models.
- [x] Diagnose `SelectedEnum` reset timing and list interaction delay in `HomeViewModel`.
- [x] Polish `ScrollableTextBlock` padding and right-pane spacing in `HomeView.xaml`.
- [x] Replace temporary theme colors with a coherent theme/resource-driven palette.
- [x] Evolve the current loading overlay into a broader reusable busy indicator experience where needed.
- [x] Expand `ExportView` into a full options-driven export screen rather than a single export button.
- [x] Update lua-language-server export generation so entries are merged into `.luarc.json` under `diagnostics.globals`.
- [x] Add constant-value search support to `HomeViewModel` search/filter flows.
- [x] Replace plain copyright and trademark text with icon-based presentation where appropriate.
- [x] Implement real API rescan behavior in `SettingsViewModel.Rescan` and refresh dependent cached data.

**Why:** Several product and UX gaps are already acknowledged in code comments and should be tracked explicitly.


