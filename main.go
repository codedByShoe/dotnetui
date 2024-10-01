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

var result SearchResult

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
	input.SetBorder(true).SetTitle("Search Package By Name").SetBorderPadding(0, 0, 1, 1)

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
			app.SetFocus(list)
			input.SetText("")
		}
	})

	// handle key events
	app.SetInputCapture(func(event *tcell.EventKey) *tcell.EventKey {
		switch event.Key() {
		case tcell.KeyTab:
			switch app.GetFocus() {
			case input:
				app.SetFocus(list)
			case list:
				app.SetFocus(input)
			}
			return nil
		case tcell.KeyBacktab:
			switch app.GetFocus() {
			case input:
				app.SetFocus(list)
			case list:
				app.SetFocus(input)
			}
			return nil
		}
		if app.GetFocus() == list {
			switch event.Rune() {
			case 'j':
				currentIndex := list.GetCurrentItem()
				if currentIndex < list.GetItemCount()-1 {
					// TODO: Move to a function
					newIdx := currentIndex + 1
					list.SetCurrentItem(newIdx)
					id, _ := list.GetItemText(newIdx)
					for k, v := range result.Data {
						if v.ID == id {
							pkg := result.Data[k]
							go showPackageDetails(pkg, details)
						}
					}
				}
				return nil
			case 'k':
				currentIndex := list.GetCurrentItem()
				// get the current item id
				if currentIndex > 0 {
					// TODO: Move to a function
					newIdx := currentIndex - 1
					list.SetCurrentItem(newIdx)
					id, _ := list.GetItemText(newIdx)
					for k, v := range result.Data {
						if v.ID == id {
							pkg := result.Data[k]
							go showPackageDetails(pkg, details)
						}
					}
				}
				return nil
			}
		}
		return event
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

func showPackageDetails(pkg NuGetPackage, details *tview.TextView) {
	detailsText := fmt.Sprintf("Package Id: %s\n", pkg.ID)
	detailsText += fmt.Sprintf("Version: %s\n", pkg.Version)
	detailsText += fmt.Sprintf("Total downloads: %d\n", pkg.TotalDownloads)
	detailsText += fmt.Sprintf("Description: %s\n", pkg.Description)

	details.SetText(detailsText)
}
