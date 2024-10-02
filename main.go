package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os/exec"
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

var (
	searchResults []NuGetPackage
	mu            sync.Mutex
)

func main() {
	app := tview.NewApplication()

	// create the main lyout
	layout := tview.NewFlex().SetDirection(tview.FlexRow)

	// create a list area for the search results
	list := tview.NewList().ShowSecondaryText(false)
	list.SetBorder(true).SetTitle("Nuget Pagkages")
	list.SetBackgroundColor(tcell.ColorDefault)

	// create the input for search
	input := tview.NewInputField()
	input.SetBackgroundColor(tcell.ColorDefault)
	input.SetFieldBackgroundColor(tcell.ColorDefault)
	input.SetBorder(true).SetTitle("Search Package By Name").SetBorderPadding(0, 0, 1, 1)

	// create the package details view
	details := tview.NewTextView().SetDynamicColors(true)
	details.SetBorder(true).SetTitle("Package Details")
	details.SetBackgroundColor(tcell.ColorDefault)

	installed := tview.NewTextView().SetDynamicColors(true)
	installed.SetBorder(true).SetTitle("Installed Packages")
	installed.SetBackgroundColor(tcell.ColorDefault)

	right := tview.NewFlex().
		SetDirection(tview.FlexRow).
		AddItem(details, 0, 1, false).
		AddItem(installed, 0, 1, false)

	// Add Components to the layout
	layout.AddItem(tview.NewFlex().
		AddItem(list, 0, 1, false).
		AddItem(right, 0, 1, false),
		0, 3, false)
	layout.AddItem(input, 3, 1, true)
	layout.SetBackgroundColor(tcell.ColorDefault)

	input.SetDoneFunc(func(key tcell.Key) {
		if key == tcell.KeyEnter {
			query := input.GetText()
			list.Clear()
			details.Clear()
			input.SetText("")
			list.AddItem("Searching Packages...", "", 0, nil)
			details.SetText("Gathering Package Details...")
			app.SetFocus(list)
			go searchPackages(query, list, details, app)
		}
	})

	// Show installed packages from csproj
	listInstalledPackages(installed)

	// handle key events
	app.SetInputCapture(func(event *tcell.EventKey) *tcell.EventKey {
		switch event.Key() {
		case tcell.KeyTab:
			switch app.GetFocus() {
			case input:
				app.SetFocus(list)
				input.SetBorderColor(tcell.ColorDefault)
				list.SetBorderColor(tcell.ColorGreen)
			case list:
				app.SetFocus(input)
				list.SetBorderColor(tcell.ColorDefault)
				input.SetBorderColor(tcell.ColorGreen)
			}
			return nil
		case tcell.KeyBacktab:
			switch app.GetFocus() {
			case input:
				app.SetFocus(list)
				input.SetBorderColor(tcell.ColorDefault)
				list.SetBorderColor(tcell.ColorGreen)
			case list:
				app.SetFocus(input)
				list.SetBorderColor(tcell.ColorDefault)
				input.SetBorderColor(tcell.ColorGreen)
			}
			return nil
		case tcell.KeyEscape:
			app.Stop()
		}

		if app.GetFocus() == list {
			switch event.Rune() {
			case 'j':
				currentIndex := list.GetCurrentItem()
				if currentIndex < list.GetItemCount()-1 {
					navigateList(currentIndex+1, list, details)
				}
				return nil
			case 'k':
				currentIndex := list.GetCurrentItem()
				// get the current item id
				if currentIndex > 0 {
					navigateList(currentIndex-1, list, details)
				}
				return nil
			}
		}
		return event
	})

	if err := app.SetRoot(layout, true).EnableMouse(true).Run(); err != nil {
		panic(err)
	}
}

func listInstalledPackages(installed *tview.TextView) {
	cmd := exec.Command("dotnet", "list", "package")
	output, err := cmd.CombinedOutput()
	if err != nil {
		installed.Clear()
		installed.SetText("No dependencies found. \n Are you in a project folder?")
		return
	}
	installed.SetText(string(output))
}

func navigateList(idx int, list *tview.List, details *tview.TextView) {
	list.SetCurrentItem(idx)
	id, _ := list.GetItemText(idx)
	for k, v := range searchResults {
		if v.ID == id {
			pkg := searchResults[k]
			go showPackageDetails(pkg, details)
		}
	}
}

func searchPackages(query string, list *tview.List, details *tview.TextView, app *tview.Application) {
	url := fmt.Sprintf("https://api-v2v3search-0.nuget.org/query?q=%s&take=100&includeDelisted=false", query)

	resp, err := http.Get(url)
	if err != nil {
		// Handle error
		app.QueueUpdateDraw(func() {
			list.Clear()
			details.SetText(fmt.Sprintf("Error fetching packages. Try again. \n Error: %v", err))
		})
		return
	}
	defer resp.Body.Close()

	var result SearchResult

	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		// Handle error
		app.QueueUpdateDraw(func() {
			list.Clear()
			details.SetText(fmt.Sprintf("Error decoding response. \n Error: %v", err))
		})
		return
	}

	mu.Lock()
	searchResults = result.Data
	mu.Unlock()

	app.QueueUpdateDraw(func() {
		list.Clear()
		for _, pkg := range result.Data {
			list.AddItem(pkg.ID, "", 0, nil)
		}
		if len(searchResults) > 0 {
			list.SetCurrentItem(0)
			showPackageDetails(searchResults[0], details)
		}
	})
}

func showPackageDetails(pkg NuGetPackage, details *tview.TextView) {
	detailsText := fmt.Sprintf("Package Id: %s\n", pkg.ID)
	detailsText += fmt.Sprintf("Version: %s\n", pkg.Version)
	detailsText += fmt.Sprintf("Total downloads: %d\n", pkg.TotalDownloads)
	detailsText += fmt.Sprintf("Description: %s\n", pkg.Description)

	details.SetText(detailsText)
}
