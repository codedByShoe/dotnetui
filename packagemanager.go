package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os/exec"
	"reflect"
	"strings"
	"sync"

	"github.com/gdamore/tcell/v2"
	"github.com/rivo/tview"
)

type NuGetPackage struct {
	ID             string `json:"id"`
	Version        string `json:"version"`
	Description    string `json:"description"`
	TotalDownloads int    `json:"totalDownloads"`
}

type SearchResult struct {
	Data []NuGetPackage `json:"data"`
}

type PackageManager struct {
	App               *tview.Application
	List              *tview.List
	Input             *tview.InputField
	Details           *tview.TextView
	Installed         *tview.List
	Legend            *tview.TextView
	Layout            *tview.Flex
	Url               string
	SearchResults     []NuGetPackage
	Query             string
	InstalledPackages []string
	Mu                sync.Mutex
}

func NewPackageManager(app *tview.Application, url string) *PackageManager {
	pm := &PackageManager{
		App:               app,
		List:              tview.NewList(),
		Input:             tview.NewInputField(),
		Details:           tview.NewTextView(),
		Installed:         tview.NewList(),
		Legend:            tview.NewTextView(),
		Layout:            tview.NewFlex(),
		Url:               url,
		SearchResults:     []NuGetPackage{},
		Query:             "",
		InstalledPackages: []string{},
		Mu:                sync.Mutex{},
	}
	return pm
}

// build out Package Manager UI

func (pm *PackageManager) buildList() {
	pm.List.ShowSecondaryText(false)
	pm.List.SetBorder(true).SetTitle("Nuget Packages")
	pm.List.SetBackgroundColor(tcell.ColorDefault)
}

func (pm *PackageManager) buildInput() {
	pm.Input.SetBackgroundColor(tcell.ColorDefault)
	pm.Input.SetFieldBackgroundColor(tcell.ColorDefault)
	pm.Input.SetBorder(true).
		SetBorderColor(tcell.ColorGreen).
		SetTitle("Search Package By Name").
		SetBorderPadding(0, 0, 1, 1)
}

func (pm *PackageManager) buildDetails() {
	pm.Details.SetDynamicColors(true)
	pm.Details.SetBorder(true).SetTitle("Package Details")
	pm.Details.SetBackgroundColor(tcell.ColorDefault)
}

func (pm *PackageManager) buildInstalled() {
	pm.Installed.ShowSecondaryText(false)
	pm.Installed.SetBorder(true).SetTitle("Installed Packages")
	pm.Installed.SetBackgroundColor(tcell.ColorDefault)
}

func (pm *PackageManager) buildLegend() {
	pm.Legend = tview.NewTextView().
		SetDynamicColors(true).
		SetTextAlign(tview.AlignCenter).
		SetText("Quit: [yellow]<Esc>[white]")

	pm.Legend.SetRect(0, 0, 0, 1)
	pm.Legend.SetBackgroundColor(tcell.ColorDefault)
}

func (pm *PackageManager) buildLayout() {
	pm.Layout.SetDirection(tview.FlexRow)
	pm.buildList()
	pm.buildInput()
	pm.buildDetails()
	pm.buildInstalled()
	pm.buildLegend()

	right := tview.NewFlex().
		SetDirection(tview.FlexRow).
		AddItem(pm.Details, 0, 1, false).
		AddItem(pm.Installed, 0, 1, false)

	// Add Components to the layout
	pm.Layout.AddItem(tview.NewFlex().
		AddItem(pm.List, 0, 1, false).
		AddItem(right, 0, 1, false), 0, 3, false)

	pm.Layout.AddItem(pm.Input, 3, 1, true)
	pm.Layout.AddItem(pm.Legend, 1, 0, false)
	pm.Layout.SetBackgroundColor(tcell.ColorDefault)
}

func (pm *PackageManager) setFocus(from, to interface{}) {
	if p, ok := to.(tview.Primitive); ok {
		pm.App.SetFocus(p)
	}

	// Helper function to set border color  NOTE: There is probably a better way to do this
	setBorderColorIfExists := func(item interface{}, color tcell.Color) {
		v := reflect.ValueOf(item)
		if v.Kind() == reflect.Ptr {
			v = v.Elem()
		}
		if v.Kind() != reflect.Struct {
			return
		}
		method := v.MethodByName("SetBorderColor")
		if method.IsValid() && !method.IsNil() {
			method.Call([]reflect.Value{reflect.ValueOf(color)})
		}
	}

	// Set border colors
	setBorderColorIfExists(from, tcell.ColorDefault)
	setBorderColorIfExists(to, tcell.ColorGreen)
}

// create app navigations

func (pm *PackageManager) handleInputSearch() {
	pm.Input.SetDoneFunc(func(key tcell.Key) {
		if key == tcell.KeyEnter {
			query := pm.Input.GetText()
			pm.List.Clear()
			pm.Details.Clear()
			pm.Input.SetText("")
			pm.List.AddItem("Searching Packages...", "", 0, nil)
			pm.Details.SetText("Gathering Package Details...")
			pm.setFocus(pm.Input, pm.List)
			go pm.searchPackages(query)
		}
	})
}

func (pm *PackageManager) updateLegend() {
	var legendText string
	switch pm.App.GetFocus() {
	case pm.List:
		legendText = "Navigate: [yellow]↑↓ or j/k[white] | Install Package: [yellow]i[white] | Quit: [yellow]<Esc>[white]"
	case pm.Installed: // Assuming this is the details view
		legendText = "Navigate: [yellow]↑↓ or j/k[white] | Update Packages: [yellow]u[white] | Remove Package: [yellow]x[white] | Quit: [yellow]<Esc>[white]"
	default:
		legendText = "Quit: [yellow]<Esc>[white]"
	}
	pm.Legend.SetText(legendText)
}

func (pm *PackageManager) handleAppNavigation() {
	// handle key events
	pm.App.SetInputCapture(func(event *tcell.EventKey) *tcell.EventKey {
		switch event.Key() {
		case tcell.KeyTab:
			switch pm.App.GetFocus() {
			case pm.Input:
				pm.setFocus(pm.Input, pm.List)
				pm.updateLegend()
			case pm.List:
				pm.setFocus(pm.List, pm.Installed)
				pm.updateLegend()
			case pm.Installed:
				pm.setFocus(pm.Installed, pm.Input)
				pm.updateLegend()
			}
			return nil
		case tcell.KeyBacktab:
			switch pm.App.GetFocus() {
			case pm.Input:
				pm.setFocus(pm.Input, pm.Installed)
				pm.updateLegend()
			case pm.Installed:
				pm.setFocus(pm.Installed, pm.List)
				pm.updateLegend()
			case pm.List:
				pm.setFocus(pm.List, pm.Input)
				pm.updateLegend()
			}
			return nil
		case tcell.KeyEscape:
			pm.App.Stop()
		}

		if pm.App.GetFocus() == pm.List {
			switch event.Rune() {
			case 'j':
				currentIndex := pm.List.GetCurrentItem()
				if currentIndex < pm.List.GetItemCount()-1 {
					pm.navigateList(currentIndex + 1)
				}
				return nil
			case 'k':
				currentIndex := pm.List.GetCurrentItem()
				// get the current item id
				if currentIndex > 0 {
					pm.navigateList(currentIndex - 1)
				}
				return nil
			case 'i':
				text, _ := pm.List.GetItemText(pm.List.GetCurrentItem())
				go pm.installPackages(text)
			}
			switch event.Key() {
			case tcell.KeyUp:
				currentIndex := pm.List.GetCurrentItem()
				// get the current item id
				if currentIndex > 0 {
					pm.navigateList(currentIndex - 1)
				}
				return nil
			case tcell.KeyDown:
				currentIndex := pm.List.GetCurrentItem()
				if currentIndex < pm.List.GetItemCount()-1 {
					pm.navigateList(currentIndex + 1)
				}
				return nil
			}
		}
		if pm.App.GetFocus() == pm.Installed {
			switch event.Rune() {
			case 'j':
				currentIndex := pm.Installed.GetCurrentItem()
				if currentIndex < pm.Installed.GetItemCount()-1 {
					pm.Installed.SetCurrentItem(currentIndex + 1)
				}
				return nil
			case 'k':
				currentIndex := pm.Installed.GetCurrentItem()
				// get the current item id
				if currentIndex > 0 {
					pm.Installed.SetCurrentItem(currentIndex - 1)
				}
				return nil
			case 'x':
				text, _ := pm.Installed.GetItemText(pm.Installed.GetCurrentItem())
				pkg := strings.Fields(text)
				go pm.uninstallPackage(pkg[0])
			}
		}
		return event
	})
}

// build out the application

func (pm *PackageManager) Run() {
	pm.buildLayout()
	pm.handleInputSearch()
	pm.listInstalledPackages()
	pm.handleAppNavigation()
	pm.App.SetRoot(pm.Layout, true).EnableMouse(true)
}

// methods for app logic

func (pm *PackageManager) installPackages(packageId string) {
	cmd := exec.Command("dotnet", "add", "package", packageId)
	_, err := cmd.CombinedOutput()
	if err != nil {
		pm.Installed.Clear()
		pm.Installed.SetText(fmt.Sprintf("Error Installing Package \n Error: %v", err))
	}
	pm.Installed.Clear()
	pm.Installed.AddItem(fmt.Sprintf("Installing %s...", packageId), "", 0, nil)

	pm.listInstalledPackages()
}

func (pm *PackageManager) uninstallPackage(packageId string) {
	cmd := exec.Command("dotnet", "remove", "package", packageId)
	output, err := cmd.CombinedOutput()
	if err != nil {
		pm.App.QueueUpdateDraw(func() {
			pm.Details.Clear()
			pm.Details.SetText(fmt.Sprintf("Error Removing Package \nError: %v", string(output)))
		})
	}
	pm.App.QueueUpdateDraw(func() {
		pm.Installed.Clear()
		pm.Installed.AddItem(fmt.Sprintf("Removing %s...", packageId), "", 0, nil)
	})
	exec.Command("dotnet", "restore")
	pm.listInstalledPackages()
}

func (pm *PackageManager) listInstalledPackages() {
	cmd := exec.Command("dotnet", "list", "package")
	output, err := cmd.CombinedOutput()
	if err != nil {
		pm.Installed.Clear()
		pm.Installed.AddItem("No dependencies found ", "", 0, nil)
		pm.Installed.AddItem("Are you in a project folder?", "", 0, nil)
		return
	}
	lines := strings.Split(string(output), "\n")
	pm.Installed.Clear()
	for _, line := range lines {
		if strings.Contains(line, ">") {
			parts := strings.Fields(line)
			formatPackageString := fmt.Sprintf("%s   Requested: %s   Resolved: %s", parts[1], parts[2], parts[3])
			pm.InstalledPackages = append(pm.InstalledPackages, formatPackageString)
			pm.Installed.AddItem(formatPackageString, "", 0, nil)
		}
	}
}

func (pm *PackageManager) navigateList(idx int) {
	pm.List.SetCurrentItem(idx)
	id, _ := pm.List.GetItemText(idx)
	for k, v := range pm.SearchResults {
		if v.ID == id {
			pkg := pm.SearchResults[k]
			go pm.showPackageDetails(pkg)
		}
	}
}

func (pm *PackageManager) searchPackages(query string) {
	url := fmt.Sprintf(pm.Url, query)

	resp, err := http.Get(url)
	if err != nil {
		// Handle error
		pm.App.QueueUpdateDraw(func() {
			pm.List.Clear()
			pm.Details.SetText("Error fetching packages. \n Check your connection.")
		})
		return
	}
	defer resp.Body.Close()

	var result SearchResult

	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		// Handle error
		pm.App.QueueUpdateDraw(func() {
			pm.List.Clear()
			pm.Details.SetText(fmt.Sprintf("Error decoding response. \n Error: %v", err))
		})
		return
	}

	pm.Mu.Lock()
	pm.SearchResults = result.Data
	pm.Mu.Unlock()

	pm.App.QueueUpdateDraw(func() {
		pm.List.Clear()
		for _, pkg := range result.Data {
			pm.List.AddItem(pkg.ID, "", 0, nil)
		}
		if len(pm.SearchResults) > 0 {
			pm.List.SetCurrentItem(0)
			pm.showPackageDetails(pm.SearchResults[0])
		}
	})
}

func (pm *PackageManager) showPackageDetails(pkg NuGetPackage) {
	detailsText := fmt.Sprintf("Package Id: %s\n", pkg.ID)
	detailsText += fmt.Sprintf("Version: %s\n", pkg.Version)
	detailsText += fmt.Sprintf("Total downloads: %d\n", pkg.TotalDownloads)
	detailsText += fmt.Sprintf("Description: %s\n", pkg.Description)

	pm.Details.SetText(detailsText)
}
