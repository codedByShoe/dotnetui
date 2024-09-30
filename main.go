package main

import (
	"encoding/json"
	"fmt"
	"net/http"

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

func main() {
	app := tview.NewApplication()

	// create the main lyout
	flex := tview.NewFlex().SetDirection(tview.FlexRow)

	// create a list area for the search results
	list := tview.NewList().ShowSecondaryText(false)
	list.SetBorder(true).SetTitle("Nuget Pagkages")
	list.SetBackgroundColor(tcell.ColorDefault)

	// create the input for search
	input := tview.NewInputField()
	input.SetBackgroundColor(tcell.ColorDefault)
	input.SetFieldBackgroundColor(tcell.ColorDefault)
	input.SetBorder(true).SetTitle("Search Packages").SetBorderPadding(0, 0, 1, 1)

	// create the package details view
	details := tview.NewTextView().SetDynamicColors(true)
	details.SetBorder(true).SetTitle("Package Details")
	details.SetBackgroundColor(tcell.ColorDefault)

	// Add Components to the layout
	flex.AddItem(tview.NewFlex().
		AddItem(list, 0, 1, false).
		AddItem(details, 0, 1, false),
		0, 3, false)
	flex.AddItem(input, 3, 1, true)
	flex.SetBackgroundColor(tcell.ColorDefault)

	input.SetDoneFunc(func(key tcell.Key) {
		if key == tcell.KeyEnter {
			query := input.GetText()
			go searchPackages(query, list, app)
		}
	})

	if err := app.SetRoot(flex, true).EnableMouse(true).Run(); err != nil {
		panic(err)
	}
}

func searchPackages(query string, list *tview.List, app *tview.Application) {
	url := fmt.Sprintf("https://api-v2v3search-0.nuget.org/query?q=%s&take=100&includeDelisted=false", query)

	resp, err := http.Get(url)
	if err != nil {
		// Handle error
		return
	}
	defer resp.Body.Close()

	var result SearchResult
	if err := json.NewDecoder(resp.Body).Decode(&result); err != nil {
		// Handle error
		return
	}

	app.QueueUpdateDraw(func() {
		list.Clear()
		for _, pkg := range result.Data {
			list.AddItem(pkg.ID, "", 0, nil)
		}
	})
}

// func showPackageDetails(packageID string, details *tview.TextView) {
// 	// In a real application, you would fetch more details about the package here
// 	details.SetText(fmt.Sprintf("Package ID: %s\n\nPress 'i' to install or 'q' to go back", packageID))
//
// 	details.SetInputCapture(func(event *tcell.EventKey) *tcell.EventKey {
// 		if event.Rune() == 'i' {
// 			go installPackage(packageID)
// 		} else if event.Rune() == 'q' {
// 			// Go back to the list
// 			// You'll need to implement this logic
// 		}
// 		return event
// 	})
// }
