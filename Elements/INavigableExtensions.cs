using System.Diagnostics.CodeAnalysis;
using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

public static class INavigableExtensions
{
    extension(INavigable self)
    {
        public void SetNeighborUp(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Up, selectable);

        public void SetNeighborUp(SelectableElement selectableElement) =>
            self.SetNeighborUp(selectableElement.SelectableComponent);

        public void ClearNeighborUp() => self.ClearNeighbor(NavigationDirection.Up);

        public bool GetNeighborUp([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Up, out selectable);

        public void SetNeighborLeft(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Left, selectable);

        public void SetNeighborLeft(SelectableElement selectableElement) =>
            self.SetNeighborLeft(selectableElement.SelectableComponent);

        public void ClearNeighborLeft() => self.ClearNeighbor(NavigationDirection.Left);

        public bool GetNeighborLeft([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Left, out selectable);

        public void SetNeighborRight(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Right, selectable);

        public void SetNeighborRight(SelectableElement selectableElement) =>
            self.SetNeighborRight(selectableElement.SelectableComponent);

        public bool GetNeighborRight([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Right, out selectable);

        public void ClearNeighborRight() => self.ClearNeighbor(NavigationDirection.Right);

        public void SetNeighborDown(Selectable selectable) =>
            self.SetNeighbor(NavigationDirection.Down, selectable);

        public void SetNeighborDown(SelectableElement selectableElement) =>
            self.SetNeighborDown(selectableElement.SelectableComponent);

        public void ClearNeighborDown() => self.ClearNeighbor(NavigationDirection.Down);

        public bool GetNeighborDown([MaybeNullWhen(false)] out Selectable selectable) =>
            self.GetSelectable(NavigationDirection.Down, out selectable);
    }
}
